using crm_backend.Data;
using crm_backend.Modules.Collaboration.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Collaboration.Services;

public class UserRoleService : IUserRoleService
{
    private readonly CrmDbContext _context;

    public UserRoleService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserRoleDto>> GetUserRolesAsync(int userId, int? companyId = null)
    {
        var query = _context.UserRoles
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .Include(ur => ur.Company)
            .Include(ur => ur.AssignedByUser)
            .Where(ur => ur.UserId == userId && ur.IsActive)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(ur => ur.CompanyId == companyId.Value);
        }

        var userRoles = await query
            .Select(ur => new UserRoleDto
            {
                Id = ur.Id,
                UserId = ur.UserId,
                UserName = ur.User.Name,
                UserEmail = ur.User.Email,
                RoleId = ur.RoleId,
                RoleName = ur.Role.Name,
                CompanyId = ur.CompanyId,
                CompanyName = ur.Company.Name,
                AssignedByUserId = ur.AssignedByUserId,
                AssignedByName = ur.AssignedByUser != null ? ur.AssignedByUser.Name : null,
                AssignedAt = ur.AssignedAt,
                IsActive = ur.IsActive
            })
            .ToListAsync();

        return userRoles;
    }

    public async Task<UserRoleDto> AssignUserRoleAsync(AssignUserRoleDto dto, int assignedByUserId)
    {
        // Verify user exists and belongs to company
        var userCompany = await _context.UserCompanies
            .AnyAsync(uc => uc.UserId == dto.UserId && uc.CompanyId == dto.CompanyId);
        if (!userCompany)
        {
            throw new InvalidOperationException("User does not belong to the specified company.");
        }

        // Verify role exists
        var role = await _context.Roles.FindAsync(dto.RoleId);
        if (role == null)
        {
            throw new InvalidOperationException($"Role with ID {dto.RoleId} does not exist.");
        }

        // Verify role belongs to company or is system role
        if (!role.IsSystemRole && role.CompanyId != dto.CompanyId)
        {
            throw new InvalidOperationException("Role does not belong to the specified company.");
        }

        // Check if role already assigned
        var existing = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId && ur.CompanyId == dto.CompanyId && ur.IsActive);
        if (existing != null)
        {
            throw new InvalidOperationException("User already has this role assigned.");
        }

        var userRole = new UserRole
        {
            UserId = dto.UserId,
            RoleId = dto.RoleId,
            CompanyId = dto.CompanyId,
            AssignedByUserId = assignedByUserId,
            AssignedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        // Load related entities for response
        await _context.Entry(userRole).Reference(ur => ur.User).LoadAsync();
        await _context.Entry(userRole).Reference(ur => ur.Role).LoadAsync();
        await _context.Entry(userRole).Reference(ur => ur.Company).LoadAsync();
        await _context.Entry(userRole).Reference(ur => ur.AssignedByUser).LoadAsync();

        return new UserRoleDto
        {
            Id = userRole.Id,
            UserId = userRole.UserId,
            UserName = userRole.User.Name,
            UserEmail = userRole.User.Email,
            RoleId = userRole.RoleId,
            RoleName = userRole.Role.Name,
            CompanyId = userRole.CompanyId,
            CompanyName = userRole.Company.Name,
            AssignedByUserId = userRole.AssignedByUserId,
            AssignedByName = userRole.AssignedByUser != null ? userRole.AssignedByUser.Name : null,
            AssignedAt = userRole.AssignedAt,
            IsActive = userRole.IsActive
        };
    }

    public async Task<bool> RemoveUserRoleAsync(int userRoleId)
    {
        var userRole = await _context.UserRoles.FindAsync(userRoleId);
        if (userRole == null) return false;

        userRole.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UserHasRoleAsync(int userId, string roleName, int companyId)
    {
        var hasRole = await _context.UserRoles
            .Include(ur => ur.Role)
            .AnyAsync(ur => ur.UserId == userId 
                && ur.CompanyId == companyId 
                && ur.IsActive 
                && ur.Role.Name == roleName);
        
        return hasRole;
    }

    public async Task<bool> UserHasPermissionAsync(int userId, string permissionName, int companyId)
    {
        var hasPermission = await _context.UserRoles
            .Include(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .Where(ur => ur.UserId == userId && ur.CompanyId == companyId && ur.IsActive)
            .SelectMany(ur => ur.Role.RolePermissions)
            .AnyAsync(rp => rp.Permission.Name == permissionName);

        return hasPermission;
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId, int companyId)
    {
        var permissions = await _context.UserRoles
            .Include(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .Where(ur => ur.UserId == userId && ur.CompanyId == companyId && ur.IsActive)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync();

        return permissions;
    }
}

