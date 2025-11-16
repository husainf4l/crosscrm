using crm_backend.Modules.Contract.DTOs;

namespace crm_backend.Modules.Contract.Services;

public interface IContractWorkflowService
{
    Task<ContractDto> TransitionContractStatusAsync(int contractId, ContractStatus newStatus, int userId, int companyId);
    Task<bool> ValidateContractStatusTransitionAsync(ContractStatus fromStatus, ContractStatus toStatus);
    Task<ContractDto> LinkContractToInvoiceAsync(int contractId, int invoiceId, int userId, int companyId);
}

