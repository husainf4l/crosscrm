using crm_backend.Data;
using crm_backend.Modules.Collaboration.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Collaboration.Services;

public class TeamService : ITeamService
{
    private readonly CrmDbContext _context;

    public TeamService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TeamDto>> GetAllTeamsAsync(int? companyId = null)
    {
        var teamsQuery = _context.Teams
            .Include(t => t.Company)
            .Include(t => t.Manager)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .AsQueryable();

        if (companyId.HasValue)
        {
            teamsQuery = teamsQuery.Where(t => t.CompanyId == companyId.Value);
        }

        var teams = await teamsQuery.ToListAsync();

        return teams.Select(team => new TeamDto
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            Type = team.Type,
            IsActive = team.IsActive,
            ManagerUserId = team.ManagerUserId,
            ManagerName = team.Manager?.Name,
            Manager = team.Manager != null ? new ManagerBasicDto
            {
                Id = team.Manager.Id,
                Name = team.Manager.Name,
                Email = team.Manager.Email
            } : null,
            CompanyId = team.CompanyId,
            CompanyName = team.Company.Name,
            MemberCount = team.Members.Count(m => m.IsActive),
            Members = team.Members.Where(m => m.IsActive).Select(m => new TeamMemberDto
            {
                Id = m.Id,
                TeamId = m.TeamId,
                TeamName = team.Name,
                UserId = m.UserId,
                UserName = m.User.Name,
                UserEmail = m.User.Email,
                Role = m.Role,
                IsActive = m.IsActive,
                JoinedAt = m.JoinedAt,
                LeftAt = m.LeftAt
            }).ToList(),
            CreatedAt = team.CreatedAt,
            UpdatedAt = team.UpdatedAt
        }).ToList();
    }

    public async Task<TeamDto?> GetTeamByIdAsync(int id)
    {
        var team = await _context.Teams
            .Include(t => t.Company)
            .Include(t => t.Manager)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null) return null;

        return new TeamDto
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            Type = team.Type,
            IsActive = team.IsActive,
            ManagerUserId = team.ManagerUserId,
            ManagerName = team.Manager != null ? team.Manager.Name : null,
            Manager = team.Manager != null ? new ManagerBasicDto
            {
                Id = team.Manager.Id,
                Name = team.Manager.Name,
                Email = team.Manager.Email
            } : null,
            CompanyId = team.CompanyId,
            CompanyName = team.Company.Name,
            MemberCount = team.Members.Count(m => m.IsActive),
            Members = team.Members.Select(m => new TeamMemberDto
            {
                Id = m.Id,
                TeamId = m.TeamId,
                TeamName = team.Name,
                UserId = m.UserId,
                UserName = m.User.Name,
                UserEmail = m.User.Email,
                Role = m.Role,
                IsActive = m.IsActive,
                JoinedAt = m.JoinedAt,
                LeftAt = m.LeftAt
            }).ToList(),
            CreatedAt = team.CreatedAt,
            UpdatedAt = team.UpdatedAt
        };
    }

    public async Task<TeamDto> CreateTeamAsync(CreateTeamDto dto)
    {
        var companyId = dto.CompanyId ?? throw new InvalidOperationException("Company ID is required");

        // Verify company exists
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId);
        if (!companyExists)
        {
            throw new InvalidOperationException($"Company with ID {companyId} does not exist.");
        }

        // Verify manager exists and belongs to company (if provided)
        if (dto.ManagerUserId.HasValue)
        {
            var userCompany = await _context.UserCompanies
                .AnyAsync(uc => uc.UserId == dto.ManagerUserId.Value && uc.CompanyId == companyId);
            if (!userCompany)
            {
                throw new InvalidOperationException("Manager does not belong to the specified company.");
            }
        }

        var team = new Team
        {
            Name = dto.Name,
            Description = dto.Description,
            Type = dto.Type,
            IsActive = true,
            ManagerUserId = dto.ManagerUserId,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        // Add initial members if provided
        if (dto.MemberUserIds != null && dto.MemberUserIds.Count > 0)
        {
            foreach (var userId in dto.MemberUserIds)
            {
                // Verify user belongs to company
                var userCompany = await _context.UserCompanies
                    .AnyAsync(uc => uc.UserId == userId && uc.CompanyId == companyId);
                if (!userCompany)
                {
                    continue; // Skip invalid users
                }

                var member = new TeamMember
                {
                    TeamId = team.Id,
                    UserId = userId,
                    Role = TeamMemberRole.Member,
                    IsActive = true,
                    JoinedAt = DateTime.UtcNow
                };
                _context.TeamMembers.Add(member);
            }
            await _context.SaveChangesAsync();
        }

        // Load related entities for response
        await _context.Entry(team).Reference(t => t.Company).LoadAsync();
        if (team.ManagerUserId.HasValue)
        {
            await _context.Entry(team).Reference(t => t.Manager).LoadAsync();
        }
        await _context.Entry(team).Collection(t => t.Members).LoadAsync();

        return new TeamDto
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            Type = team.Type,
            IsActive = team.IsActive,
            ManagerUserId = team.ManagerUserId,
            ManagerName = team.Manager != null ? team.Manager.Name : null,
            CompanyId = team.CompanyId,
            CompanyName = team.Company.Name,
            MemberCount = team.Members.Count(m => m.IsActive),
            CreatedAt = team.CreatedAt,
            UpdatedAt = team.UpdatedAt
        };
    }

    public async Task<TeamDto?> UpdateTeamAsync(int id, UpdateTeamDto dto)
    {
        var team = await _context.Teams
            .Include(t => t.Company)
            .Include(t => t.Manager)
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null) return null;

        // Note: CompanyId is intentionally not updated as teams cannot change companies

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            team.Name = dto.Name;
        }

        if (dto.Description != null)
        {
            team.Description = dto.Description;
        }

        if (dto.Type.HasValue)
        {
            team.Type = dto.Type.Value;
        }

        if (dto.IsActive.HasValue)
        {
            team.IsActive = dto.IsActive.Value;
        }

        if (dto.ManagerUserId.HasValue)
        {
            // Verify manager belongs to company
            var userCompany = await _context.UserCompanies
                .AnyAsync(uc => uc.UserId == dto.ManagerUserId.Value && uc.CompanyId == team.CompanyId);
            if (!userCompany)
            {
                throw new InvalidOperationException("Manager does not belong to the team's company.");
            }
            team.ManagerUserId = dto.ManagerUserId;
        }

        team.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Reload manager if changed
        if (dto.ManagerUserId.HasValue)
        {
            await _context.Entry(team).Reference(t => t.Manager).LoadAsync();
        }

        return new TeamDto
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            Type = team.Type,
            IsActive = team.IsActive,
            ManagerUserId = team.ManagerUserId,
            ManagerName = team.Manager != null ? team.Manager.Name : null,
            CompanyId = team.CompanyId,
            CompanyName = team.Company.Name,
            MemberCount = team.Members.Count(m => m.IsActive),
            CreatedAt = team.CreatedAt,
            UpdatedAt = team.UpdatedAt
        };
    }

    public async Task<bool> DeleteTeamAsync(int id)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null) return false;

        // Remove all members first
        var members = await _context.TeamMembers.Where(tm => tm.TeamId == id).ToListAsync();
        _context.TeamMembers.RemoveRange(members);

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TeamMemberDto> AddTeamMemberAsync(AddTeamMemberDto dto)
    {
        // Verify team exists
        var team = await _context.Teams.FindAsync(dto.TeamId);
        if (team == null)
        {
            throw new InvalidOperationException($"Team with ID {dto.TeamId} does not exist.");
        }

        // Verify user exists and belongs to company
        var userCompany = await _context.UserCompanies
            .AnyAsync(uc => uc.UserId == dto.UserId && uc.CompanyId == team.CompanyId);
        if (!userCompany)
        {
            throw new InvalidOperationException("User does not belong to the team's company.");
        }

        // Check if member already exists
        var existingMember = await _context.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == dto.TeamId && tm.UserId == dto.UserId && tm.IsActive);
        if (existingMember != null)
        {
            throw new InvalidOperationException("User is already a member of this team.");
        }

        var member = new TeamMember
        {
            TeamId = dto.TeamId,
            UserId = dto.UserId,
            Role = dto.Role,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };

        _context.TeamMembers.Add(member);
        await _context.SaveChangesAsync();

        // Load related entities for response
        await _context.Entry(member).Reference(m => m.Team).LoadAsync();
        await _context.Entry(member).Reference(m => m.User).LoadAsync();

        return new TeamMemberDto
        {
            Id = member.Id,
            TeamId = member.TeamId,
            TeamName = member.Team.Name,
            UserId = member.UserId,
            UserName = member.User.Name,
            UserEmail = member.User.Email,
            Role = member.Role,
            IsActive = member.IsActive,
            JoinedAt = member.JoinedAt,
            LeftAt = member.LeftAt
        };
    }

    public async Task<TeamMemberDto?> UpdateTeamMemberAsync(int memberId, UpdateTeamMemberDto dto)
    {
        var member = await _context.TeamMembers
            .Include(tm => tm.Team)
            .Include(tm => tm.User)
            .FirstOrDefaultAsync(tm => tm.Id == memberId);

        if (member == null) return null;

        if (dto.Role.HasValue)
        {
            member.Role = dto.Role.Value;
        }

        if (dto.IsActive.HasValue)
        {
            member.IsActive = dto.IsActive.Value;
            if (!dto.IsActive.Value && member.LeftAt == null)
            {
                member.LeftAt = DateTime.UtcNow;
            }
            else if (dto.IsActive.Value && member.LeftAt != null)
            {
                member.LeftAt = null;
            }
        }

        await _context.SaveChangesAsync();

        return new TeamMemberDto
        {
            Id = member.Id,
            TeamId = member.TeamId,
            TeamName = member.Team.Name,
            UserId = member.UserId,
            UserName = member.User.Name,
            UserEmail = member.User.Email,
            Role = member.Role,
            IsActive = member.IsActive,
            JoinedAt = member.JoinedAt,
            LeftAt = member.LeftAt
        };
    }

    public async Task<bool> RemoveTeamMemberAsync(int memberId)
    {
        var member = await _context.TeamMembers.FindAsync(memberId);
        if (member == null) return false;

        member.IsActive = false;
        member.LeftAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveTeamMemberAsync(int teamId, int userId)
    {
        var member = await _context.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId && tm.IsActive);

        if (member == null) return false;

        member.IsActive = false;
        member.LeftAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<TeamMemberDto>> GetTeamMembersAsync(int teamId)
    {
        var members = await _context.TeamMembers
            .Include(tm => tm.Team)
            .Include(tm => tm.User)
            .Where(tm => tm.TeamId == teamId)
            .Select(tm => new TeamMemberDto
            {
                Id = tm.Id,
                TeamId = tm.TeamId,
                TeamName = tm.Team.Name,
                UserId = tm.UserId,
                UserName = tm.User.Name,
                UserEmail = tm.User.Email,
                Role = tm.Role,
                IsActive = tm.IsActive,
                JoinedAt = tm.JoinedAt,
                LeftAt = tm.LeftAt
            })
            .ToListAsync();

        return members;
    }

    public async Task<IEnumerable<TeamDto>> GetTeamsByUserAsync(int userId, int companyId)
    {
        var teams = await _context.TeamMembers
            .Include(tm => tm.Team)
            .ThenInclude(t => t.Company)
            .Include(tm => tm.Team)
            .ThenInclude(t => t.Manager)
            .Include(tm => tm.Team)
            .ThenInclude(t => t.Members)
            .Where(tm => tm.UserId == userId && tm.Team.CompanyId == companyId && tm.IsActive)
            .Select(tm => new TeamDto
            {
                Id = tm.Team.Id,
                Name = tm.Team.Name,
                Description = tm.Team.Description,
                Type = tm.Team.Type,
                IsActive = tm.Team.IsActive,
                ManagerUserId = tm.Team.ManagerUserId,
                ManagerName = tm.Team.Manager != null ? tm.Team.Manager.Name : null,
                CompanyId = tm.Team.CompanyId,
                CompanyName = tm.Team.Company.Name,
                MemberCount = tm.Team.Members.Count(m => m.IsActive),
                CreatedAt = tm.Team.CreatedAt,
                UpdatedAt = tm.Team.UpdatedAt
            })
            .ToListAsync();

        return teams;
    }
}

