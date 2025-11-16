using crm_backend.Data;
using crm_backend.Modules.User.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.User.Services;

public class UserService : IUserService
{
    private readonly CrmDbContext _context;

    public UserService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .Include(u => u.Company)
            .Include(u => u.UserCompanies)
                .ThenInclude(uc => uc.Company)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                Avatar = u.Avatar,
                CreatedAt = u.CreatedAt,
                CompanyId = u.CompanyId,
                CompanyName = u.Company != null ? u.Company.Name : null,
                Companies = u.UserCompanies.Select(uc => new UserCompanyDto
                {
                    CompanyId = uc.CompanyId,
                    CompanyName = uc.Company != null ? uc.Company.Name : "Unknown Company",
                    IsActive = uc.IsActive,
                    JoinedAt = uc.JoinedAt
                }).ToList()
            })
            .ToListAsync();

        return users;
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users
            .Include(u => u.Company)
            .Include(u => u.Manager)
            .Include(u => u.DirectReports)
            .Include(u => u.UserCompanies)
                .ThenInclude(uc => uc.Company)
            .Include(u => u.TeamMemberships)
                .ThenInclude(tm => tm.Team)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return null;

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Avatar = user.Avatar,
            CreatedAt = user.CreatedAt,
            CompanyId = user.CompanyId,
            CompanyName = user.Company?.Name ?? string.Empty,
            ManagerId = user.ManagerId,
            Manager = user.Manager != null ? new UserBasicDto
            {
                Id = user.Manager.Id,
                Name = user.Manager.Name,
                Email = user.Manager.Email,
                Avatar = user.Manager.Avatar
            } : null,
            DirectReports = user.DirectReports.Select(dr => new UserBasicDto
            {
                Id = dr.Id,
                Name = dr.Name,
                Email = dr.Email,
                Avatar = dr.Avatar
            }).ToList(),
            Companies = user.UserCompanies.Select(uc => new UserCompanyDto
            {
                CompanyId = uc.CompanyId,
                CompanyName = uc.Company?.Name ?? "Unknown Company",
                IsActive = uc.IsActive,
                JoinedAt = uc.JoinedAt
            }).ToList(),
            Teams = user.TeamMemberships.Select(tm => new TeamMembershipDto
            {
                TeamId = tm.TeamId,
                TeamName = tm.Team?.Name ?? "Unknown Team",
                Role = tm.Role.ToString(),
                IsActive = tm.IsActive,
                JoinedAt = tm.JoinedAt
            }).ToList(),
            Roles = user.UserRoles.Select(ur => new UserRoleAssignmentDto
            {
                RoleId = ur.RoleId,
                RoleName = ur.Role?.Name ?? "Unknown Role",
                IsActive = ur.IsActive,
                AssignedAt = ur.AssignedAt
            }).ToList()
        };
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
    {
        // Check if email already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"A user with email '{dto.Email}' already exists.");
        }

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Avatar = dto.Avatar
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // If company is specified, create the relationship and set as active
        if (dto.CompanyId.HasValue)
        {
            var userCompany = new crm_backend.Modules.User.UserCompany
            {
                UserId = user.Id,
                CompanyId = dto.CompanyId.Value,
                IsActive = true
            };
            _context.UserCompanies.Add(userCompany);
            user.CompanyId = dto.CompanyId.Value;
            await _context.SaveChangesAsync();
        }

        // Load company and user companies for response
        await _context.Entry(user).Reference(u => u.Company).LoadAsync();
        await _context.Entry(user).Collection(u => u.UserCompanies).Query().Include(uc => uc.Company).LoadAsync();

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Avatar = user.Avatar,
            CreatedAt = user.CreatedAt,
            CompanyId = user.CompanyId,
            CompanyName = user.Company?.Name ?? string.Empty,
            Companies = user.UserCompanies.Select(uc => new UserCompanyDto
            {
                CompanyId = uc.CompanyId,
                CompanyName = uc.Company != null ? uc.Company.Name! : "Unknown Company",
                IsActive = uc.IsActive,
                JoinedAt = uc.JoinedAt
            }).ToList()
        };
    }

    public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return null;

        // Check if email is being changed and if the new email already exists
        if (user.Email != dto.Email)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"A user with email '{dto.Email}' already exists.");
            }
        }

        user.Name = dto.Name;
        user.Email = dto.Email;
        user.Phone = dto.Phone;
        user.Avatar = dto.Avatar;
        user.ManagerId = dto.ManagerId;
        user.CompanyId = dto.CompanyId;

        await _context.SaveChangesAsync();

        // Load related data
        await _context.Entry(user).Reference(u => u.Company).LoadAsync();
        await _context.Entry(user).Reference(u => u.Manager).LoadAsync();
        await _context.Entry(user).Collection(u => u.DirectReports).LoadAsync();
        await _context.Entry(user).Collection(u => u.UserCompanies).Query().Include(uc => uc.Company).LoadAsync();
        await _context.Entry(user).Collection(u => u.TeamMemberships).Query().Include(tm => tm.Team).LoadAsync();
        await _context.Entry(user).Collection(u => u.UserRoles).Query().Include(ur => ur.Role).LoadAsync();

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Avatar = user.Avatar,
            CreatedAt = user.CreatedAt,
            CompanyId = user.CompanyId,
            CompanyName = user.Company?.Name ?? string.Empty,
            ManagerId = user.ManagerId,
            Manager = user.Manager != null ? new UserBasicDto
            {
                Id = user.Manager.Id,
                Name = user.Manager.Name,
                Email = user.Manager.Email,
                Avatar = user.Manager.Avatar
            } : null,
            DirectReports = user.DirectReports.Select(dr => new UserBasicDto
            {
                Id = dr.Id,
                Name = dr.Name,
                Email = dr.Email,
                Avatar = dr.Avatar
            }).ToList(),
            Companies = user.UserCompanies.Select(uc => new UserCompanyDto
            {
                CompanyId = uc.CompanyId,
                CompanyName = uc.Company != null ? uc.Company.Name! : "Unknown Company",
                IsActive = uc.IsActive,
                JoinedAt = uc.JoinedAt
            }).ToList(),
            Teams = user.TeamMemberships.Select(tm => new TeamMembershipDto
            {
                TeamId = tm.TeamId,
                TeamName = tm.Team?.Name ?? "Unknown Team",
                Role = tm.Role.ToString(),
                IsActive = tm.IsActive,
                JoinedAt = tm.JoinedAt
            }).ToList(),
            Roles = user.UserRoles.Select(ur => new UserRoleAssignmentDto
            {
                RoleId = ur.RoleId,
                RoleName = ur.Role?.Name ?? "Unknown Role",
                IsActive = ur.IsActive,
                AssignedAt = ur.AssignedAt
            }).ToList()
        };
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}