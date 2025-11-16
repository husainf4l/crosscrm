using crm_backend.Data;
using crm_backend.Modules.Collaboration.Services;
using crm_backend.Modules.Financial.DTOs;
using crm_backend.Modules.Financial.Services;
using crm_backend.Modules.Opportunity.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Opportunity.Services;

public class OpportunityWorkflowService : IOpportunityWorkflowService
{
    private readonly CrmDbContext _context;
    private readonly IQuoteService _quoteService;
    private readonly IActivityTimelineService _activityTimelineService;
    private readonly INotificationService _notificationService;

    public OpportunityWorkflowService(
        CrmDbContext context,
        IQuoteService quoteService,
        IActivityTimelineService activityTimelineService,
        INotificationService notificationService)
    {
        _context = context;
        _quoteService = quoteService;
        _activityTimelineService = activityTimelineService;
        _notificationService = notificationService;
    }

    public async Task<bool> ValidateStatusTransitionAsync(OpportunityStatus fromStatus, OpportunityStatus toStatus)
    {
        // Define valid status transitions
        var validTransitions = new Dictionary<OpportunityStatus, List<OpportunityStatus>>
        {
            { OpportunityStatus.Open, new List<OpportunityStatus> { OpportunityStatus.Won, OpportunityStatus.Lost, OpportunityStatus.Abandoned } },
            { OpportunityStatus.Won, new List<OpportunityStatus>() }, // Terminal state
            { OpportunityStatus.Lost, new List<OpportunityStatus>() }, // Terminal state
            { OpportunityStatus.Abandoned, new List<OpportunityStatus> { OpportunityStatus.Open } } // Can reopen
        };

        if (!validTransitions.ContainsKey(fromStatus))
            return false;

        return validTransitions[fromStatus].Contains(toStatus);
    }

    public async Task<OpportunityDto> TransitionOpportunityStatusAsync(int opportunityId, OpportunityStatus newStatus, string? reason, int userId, int companyId)
    {
        var opportunity = await _context.Opportunities
            .Include(o => o.Customer)
            .Include(o => o.AssignedUser)
            .Include(o => o.AssignedTeam)
            .FirstOrDefaultAsync(o => o.Id == opportunityId && o.CompanyId == companyId);

        if (opportunity == null)
            throw new InvalidOperationException("Opportunity not found");

        var oldStatus = opportunity.Status;

        // Validate transition
        if (!await ValidateStatusTransitionAsync(oldStatus, newStatus))
            throw new InvalidOperationException($"Invalid status transition from {oldStatus} to {newStatus}");

        // Update status
        opportunity.Status = newStatus;
        opportunity.UpdatedAt = DateTime.UtcNow;

        // Handle specific status changes
        if (newStatus == OpportunityStatus.Won)
        {
            opportunity.WonAt = DateTime.UtcNow;
            opportunity.ActualCloseDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(reason))
                opportunity.WinReason = reason;

            // Auto-create Quote if not exists
            var existingQuotes = await GetQuotesByOpportunityAsync(opportunityId, companyId);
            if (!existingQuotes.Any())
            {
                // Create a draft quote from opportunity
                var quoteDto = new CreateQuoteDto
                {
                    CustomerId = opportunity.CustomerId,
                    OpportunityId = opportunityId,
                    Title = $"Quote for {opportunity.Name}",
                    Description = opportunity.Description,
                    Currency = opportunity.Currency,
                    CompanyId = companyId,
                    CreatedByUserId = userId
                };
                await _quoteService.CreateQuoteAsync(quoteDto);
            }

            // Update customer status
            opportunity.Customer.Status = "active";
        }
        else if (newStatus == OpportunityStatus.Lost)
        {
            opportunity.LostAt = DateTime.UtcNow;
            opportunity.ActualCloseDate = DateTime.UtcNow;
            if (string.IsNullOrEmpty(reason))
                throw new ArgumentException("Lost reason is required when marking opportunity as lost");
            opportunity.LostReason = reason;
        }

        await _context.SaveChangesAsync();

        // Log activity
        await _activityTimelineService.LogActivityAsync(new Collaboration.DTOs.CreateActivityTimelineDto
        {
            EntityType = "Opportunity",
            EntityId = opportunityId,
            Type = "StatusChange",
            Description = $"Opportunity status changed from {oldStatus} to {newStatus}" + (reason != null ? $": {reason}" : "")
        }, companyId, userId);

        // Send notifications
        var notificationTitle = newStatus == OpportunityStatus.Won ? "Opportunity Won!" : "Opportunity Status Changed";
        var notificationMessage = $"Opportunity '{opportunity.Name}' status changed to {newStatus}";

        if (opportunity.AssignedUserId.HasValue)
        {
            await _notificationService.CreateNotificationAsync(new Collaboration.DTOs.CreateNotificationDto
            {
                UserId = opportunity.AssignedUserId.Value,
                Title = notificationTitle,
                Message = notificationMessage,
                Type = "Activity",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                Priority = newStatus == OpportunityStatus.Won ? "High" : "Medium"
            }, companyId);
        }

        // Map to DTO
        return new OpportunityDto
        {
            Id = opportunity.Id,
            Name = opportunity.Name,
            Description = opportunity.Description,
            Amount = opportunity.Amount,
            Currency = opportunity.Currency,
            Probability = opportunity.Probability,
            Status = opportunity.Status,
            LostReason = opportunity.LostReason,
            WinReason = opportunity.WinReason,
            CustomerId = opportunity.CustomerId,
            CustomerName = opportunity.Customer.Name,
            AssignedUserId = opportunity.AssignedUserId,
            AssignedUserName = opportunity.AssignedUser?.Name,
            CompanyId = opportunity.CompanyId,
            CreatedAt = opportunity.CreatedAt,
            UpdatedAt = opportunity.UpdatedAt,
            WonAt = opportunity.WonAt,
            LostAt = opportunity.LostAt
        };
    }

    public async Task<OpportunityDto> MarkOpportunityAsWonAsync(int opportunityId, string? winReason, int userId, int companyId)
    {
        return await TransitionOpportunityStatusAsync(opportunityId, OpportunityStatus.Won, winReason, userId, companyId);
    }

    public async Task<OpportunityDto> MarkOpportunityAsLostAsync(int opportunityId, string lostReason, int userId, int companyId)
    {
        return await TransitionOpportunityStatusAsync(opportunityId, OpportunityStatus.Lost, lostReason, userId, companyId);
    }

    public async Task<IEnumerable<QuoteDto>> GetQuotesByOpportunityAsync(int opportunityId, int companyId)
    {
        var opportunity = await _context.Opportunities
            .FirstOrDefaultAsync(o => o.Id == opportunityId && o.CompanyId == companyId);

        if (opportunity == null)
            throw new InvalidOperationException("Opportunity not found");

        var allQuotes = await _quoteService.GetAllQuotesAsync(companyId);
        return allQuotes.Where(q => q.OpportunityId == opportunityId);
    }
}

