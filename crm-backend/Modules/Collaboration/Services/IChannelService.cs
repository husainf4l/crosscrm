using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.Services;

public interface IChannelService
{
    Task<IEnumerable<ChannelDto>> GetChannelsByCompanyAsync(int companyId, int? userId = null);
    Task<IEnumerable<ChannelDto>> GetChannelsByTeamAsync(int teamId, int companyId);
    Task<IEnumerable<ChannelDto>> GetChannelsByCustomerAsync(int customerId, int companyId);
    Task<ChannelDto?> GetChannelByIdAsync(int id, int? userId = null);
    Task<ChannelDto> CreateChannelAsync(CreateChannelDto dto, int companyId, int createdByUserId);
    Task<ChannelDto?> UpdateChannelAsync(int id, UpdateChannelDto dto, int companyId);
    Task<bool> DeleteChannelAsync(int id, int companyId);
    Task<ChannelMemberDto> AddChannelMemberAsync(AddChannelMemberDto dto, int companyId);
    Task<bool> RemoveChannelMemberAsync(int channelId, int userId, int companyId);
    Task<IEnumerable<ChannelMemberDto>> GetChannelMembersAsync(int channelId, int companyId);
    Task<int> GetUnreadCountAsync(int channelId, int userId, int companyId);
    Task<bool> MarkChannelAsReadAsync(int channelId, int userId, int companyId);
}

