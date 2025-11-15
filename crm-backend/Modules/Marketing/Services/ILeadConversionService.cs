using crm_backend.Modules.Customer.DTOs;
using crm_backend.Modules.Marketing.DTOs;
using crm_backend.Modules.Opportunity.DTOs;

namespace crm_backend.Modules.Marketing.Services;

public interface ILeadConversionService
{
    Task<LeadConversionResultDto> ConvertLeadToCustomerAndOpportunityAsync(int leadId, ConvertLeadDto dto, int userId, int companyId);
    Task<int> CalculateLeadScoreAsync(int leadId, int companyId);
}

public class LeadConversionResultDto
{
    public LeadDto Lead { get; set; } = null!;
    public CustomerDto? Customer { get; set; }
    public OpportunityDto? Opportunity { get; set; }
}

