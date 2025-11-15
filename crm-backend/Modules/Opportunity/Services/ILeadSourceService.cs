using crm_backend.Modules.Opportunity.DTOs;

namespace crm_backend.Modules.Opportunity.Services;

public interface ILeadSourceService
{
    Task<IEnumerable<LeadSourceDto>> GetAllLeadSourcesAsync(int? companyId = null);
    Task<LeadSourceDto?> GetLeadSourceByIdAsync(int id);
    Task<LeadSourceDto> CreateLeadSourceAsync(CreateLeadSourceDto dto);
    Task<LeadSourceDto?> UpdateLeadSourceAsync(int id, UpdateLeadSourceDto dto);
    Task<bool> DeleteLeadSourceAsync(int id);
}

