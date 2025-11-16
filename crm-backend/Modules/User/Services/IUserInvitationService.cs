using crm_backend.Modules.User.DTOs;

namespace crm_backend.Modules.User.Services;

public interface IUserInvitationService
{
    Task<UserInvitationDto> InviteUserAsync(InviteUserDto inviteDto, int invitedByUserId);
    Task<InvitationResponseDto> AcceptInvitationAsync(AcceptInvitationDto acceptDto);
    Task<UserInvitationDto?> GetInvitationByTokenAsync(string token);
    Task<IEnumerable<UserInvitationDto>> GetCompanyInvitationsAsync(int companyId);
    Task<IEnumerable<UserInvitationDto>> GetUserInvitationsAsync(int userId);
    Task<bool> CancelInvitationAsync(int invitationId, int userId);
    Task<bool> ResendInvitationAsync(int invitationId, int userId);
    Task CleanupExpiredInvitationsAsync();
}
