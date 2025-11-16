using crm_backend.Modules.Marketing.DTOs;

namespace crm_backend.Modules.Marketing.Services;

public interface ICampaignService
{
    Task<IEnumerable<CampaignDto>> GetAllCampaignsAsync(int? companyId = null);
    Task<CampaignDto?> GetCampaignByIdAsync(int id);
    Task<CampaignDto> CreateCampaignAsync(CreateCampaignDto dto);
    Task<CampaignDto?> UpdateCampaignAsync(int id, UpdateCampaignDto dto);
    Task<bool> DeleteCampaignAsync(int id);
    Task<CampaignMemberDto> AddCampaignMemberAsync(CreateCampaignMemberDto dto);
    Task<bool> RemoveCampaignMemberAsync(int memberId);
    Task<IEnumerable<CampaignMemberDto>> GetCampaignMembersAsync(int campaignId);
    Task UpdateCampaignMetricsAsync(int campaignId);
}

