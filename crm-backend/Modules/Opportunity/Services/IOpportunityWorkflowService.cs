using crm_backend.Modules.Financial.DTOs;
using crm_backend.Modules.Opportunity.DTOs;

namespace crm_backend.Modules.Opportunity.Services;

public interface IOpportunityWorkflowService
{
    Task<OpportunityDto> TransitionOpportunityStatusAsync(int opportunityId, OpportunityStatus newStatus, string? reason, int userId, int companyId);
    Task<OpportunityDto> MarkOpportunityAsWonAsync(int opportunityId, string? winReason, int userId, int companyId);
    Task<OpportunityDto> MarkOpportunityAsLostAsync(int opportunityId, string lostReason, int userId, int companyId);
    Task<bool> ValidateStatusTransitionAsync(OpportunityStatus fromStatus, OpportunityStatus toStatus);
    Task<IEnumerable<QuoteDto>> GetQuotesByOpportunityAsync(int opportunityId, int companyId);
}

