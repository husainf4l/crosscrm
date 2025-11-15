using crm_backend.Modules.Marketing.DTOs;

namespace crm_backend.Modules.Marketing.Services;

public interface ILeadService
{
    Task<IEnumerable<LeadDto>> GetAllLeadsAsync(int? companyId = null);
    Task<LeadDto?> GetLeadByIdAsync(int id);
    Task<LeadDto> CreateLeadAsync(CreateLeadDto dto);
    Task<LeadDto?> UpdateLeadAsync(int id, UpdateLeadDto dto);
    Task<bool> DeleteLeadAsync(int id);
    Task<LeadDto> ConvertLeadToCustomerAsync(int leadId, ConvertLeadDto? dto = null);
    Task<LeadDto> ConvertLeadToOpportunityAsync(int leadId, ConvertLeadDto? dto = null);
}

