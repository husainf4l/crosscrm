using crm_backend.Data;
using crm_backend.Modules.Collaboration.Services;
using crm_backend.Modules.Contract.DTOs;
using crm_backend.Modules.Opportunity;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Contract.Services;

public class ContractWorkflowService : IContractWorkflowService
{
    private readonly CrmDbContext _context;
    private readonly IContractService _contractService;
    private readonly IActivityTimelineService _activityTimelineService;

    public ContractWorkflowService(
        CrmDbContext context,
        IContractService contractService,
        IActivityTimelineService activityTimelineService)
    {
        _context = context;
        _contractService = contractService;
        _activityTimelineService = activityTimelineService;
    }

    public async Task<bool> ValidateContractStatusTransitionAsync(ContractStatus fromStatus, ContractStatus toStatus)
    {
        // Define valid status transitions
        var validTransitions = new Dictionary<ContractStatus, List<ContractStatus>>
        {
            { ContractStatus.Draft, new List<ContractStatus> { ContractStatus.Sent, ContractStatus.Cancelled } },
            { ContractStatus.Sent, new List<ContractStatus> { ContractStatus.Signed, ContractStatus.Cancelled } },
            { ContractStatus.Signed, new List<ContractStatus> { ContractStatus.Active } },
            { ContractStatus.Active, new List<ContractStatus> { ContractStatus.Expired, ContractStatus.Renewed, ContractStatus.Cancelled } },
            { ContractStatus.Expired, new List<ContractStatus>() }, // Terminal state
            { ContractStatus.Cancelled, new List<ContractStatus>() }, // Terminal state
            { ContractStatus.Renewed, new List<ContractStatus>() } // Terminal state (new contract created)
        };

        if (!validTransitions.ContainsKey(fromStatus))
            return false;

        return validTransitions[fromStatus].Contains(toStatus);
    }

    public async Task<ContractDto> TransitionContractStatusAsync(int contractId, ContractStatus newStatus, int userId, int companyId)
    {
        var contract = await _context.Contracts
            .Include(c => c.Customer)
            .Include(c => c.Opportunity)
            .FirstOrDefaultAsync(c => c.Id == contractId && c.CompanyId == companyId);

        if (contract == null)
            throw new InvalidOperationException("Contract not found");

        var oldStatus = contract.Status;

        // Validate transition
        if (!await ValidateContractStatusTransitionAsync(oldStatus, newStatus))
            throw new InvalidOperationException($"Invalid status transition from {oldStatus} to {newStatus}");

        // Business rule: Contract can only be created from Won Opportunity
        if (newStatus == ContractStatus.Draft && contract.OpportunityId.HasValue)
        {
            var opportunity = await _context.Opportunities.FindAsync(contract.OpportunityId.Value);
            if (opportunity != null && opportunity.Status != OpportunityStatus.Won)
                throw new InvalidOperationException("Cannot create contract for opportunity that is not Won.");
        }

        // Update status
        contract.Status = newStatus;
        contract.UpdatedAt = DateTime.UtcNow;

        // Handle specific status changes
        if (newStatus == ContractStatus.Signed && contract.SignedAt == null)
        {
            contract.SignedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Log activity
        await _activityTimelineService.LogActivityAsync(new Collaboration.DTOs.CreateActivityTimelineDto
        {
            EntityType = "Contract",
            EntityId = contractId,
            Type = "StatusChange",
            Description = $"Contract status changed from {oldStatus} to {newStatus}"
        }, companyId, userId);

        return await _contractService.GetContractByIdAsync(contractId)
            ?? throw new InvalidOperationException("Failed to retrieve updated contract");
    }

    public async Task<ContractDto> LinkContractToInvoiceAsync(int contractId, int invoiceId, int userId, int companyId)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == contractId && c.CompanyId == companyId);

        if (contract == null)
            throw new InvalidOperationException("Contract not found");

        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.CompanyId == companyId);

        if (invoice == null)
            throw new InvalidOperationException("Invoice not found");

        // Business rule: Contract and Invoice must belong to same customer
        if (contract.CustomerId != invoice.CustomerId)
            throw new InvalidOperationException("Contract and Invoice must belong to the same customer");

        // Link contract to invoice
        contract.InvoiceId = invoiceId;
        contract.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Log activity
        await _activityTimelineService.LogActivityAsync(new Collaboration.DTOs.CreateActivityTimelineDto
        {
            EntityType = "Contract",
            EntityId = contractId,
            Type = "Linked",
            Description = $"Contract linked to Invoice {invoice.InvoiceNumber}"
        }, companyId, userId);

        return await _contractService.GetContractByIdAsync(contractId)
            ?? throw new InvalidOperationException("Failed to retrieve updated contract");
    }
}

