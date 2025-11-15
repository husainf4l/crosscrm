using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.Services;

public interface ITeamService
{
    Task<IEnumerable<TeamDto>> GetAllTeamsAsync(int? companyId = null);
    Task<TeamDto?> GetTeamByIdAsync(int id);
    Task<TeamDto> CreateTeamAsync(CreateTeamDto dto);
    Task<TeamDto?> UpdateTeamAsync(int id, UpdateTeamDto dto);
    Task<bool> DeleteTeamAsync(int id);
    Task<TeamMemberDto> AddTeamMemberAsync(AddTeamMemberDto dto);
    Task<TeamMemberDto?> UpdateTeamMemberAsync(int memberId, UpdateTeamMemberDto dto);
    Task<bool> RemoveTeamMemberAsync(int memberId);
    Task<IEnumerable<TeamMemberDto>> GetTeamMembersAsync(int teamId);
    Task<IEnumerable<TeamDto>> GetTeamsByUserAsync(int userId, int companyId);
}

