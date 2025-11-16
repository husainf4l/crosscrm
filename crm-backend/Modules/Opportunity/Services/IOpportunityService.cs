using crm_backend.Modules.Opportunity.DTOs;

namespace crm_backend.Modules.Opportunity.Services;

public interface IOpportunityService
{
    Task<IEnumerable<OpportunityDto>> GetAllOpportunitiesAsync(int? companyId = null);
    Task<OpportunityDto?> GetOpportunityByIdAsync(int id);
    Task<OpportunityDto> CreateOpportunityAsync(CreateOpportunityDto dto);
    Task<OpportunityDto?> UpdateOpportunityAsync(int id, UpdateOpportunityDto dto);
    Task<bool> DeleteOpportunityAsync(int id);
    Task<OpportunityDto?> MoveOpportunityToStageAsync(int id, int pipelineStageId);
    Task<decimal> CalculateWeightedPipelineValueAsync(int companyId);
}

