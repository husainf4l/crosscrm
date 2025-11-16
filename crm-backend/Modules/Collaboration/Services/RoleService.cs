using crm_backend.Data;
using crm_backend.Modules.Collaboration.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Collaboration.Services;

public class RoleService : IRoleService
{
    private readonly CrmDbContext _context;

    public RoleService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync(int? companyId = null)
    {
        var query = _context.Roles
            .Include(r => r.Company)
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(r => r.CompanyId == companyId || r.IsSystemRole);
        }

        var roles = await query
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsSystemRole = r.IsSystemRole,
                CompanyId = r.CompanyId,
                CompanyName = r.Company != null ? r.Company.Name : null,
                UserCount = r.UserRoles.Count(ur => ur.IsActive),
                Permissions = r.RolePermissions.Select(rp => rp.Permission.Name).ToList(),
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync();

        return roles;
    }

    public async Task<RoleDto?> GetRoleByIdAsync(int id)
    {
        var role = await _context.Roles
            .Include(r => r.Company)
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null) return null;

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            CompanyId = role.CompanyId,
            CompanyName = role.Company != null ? role.Company.Name : null,
            UserCount = role.UserRoles.Count(ur => ur.IsActive),
            Permissions = role.RolePermissions.Select(rp => rp.Permission.Name).ToList(),
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        };
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
    {
        var companyId = dto.CompanyId;

        // Verify company exists if provided
        if (companyId.HasValue)
        {
            var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId.Value);
            if (!companyExists)
            {
                throw new InvalidOperationException($"Company with ID {companyId.Value} does not exist.");
            }
        }

        // Check if role name already exists for this company
        var existingRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == dto.Name &&
                (r.CompanyId == companyId || (r.IsSystemRole && !companyId.HasValue)));
        if (existingRole != null)
        {
            throw new InvalidOperationException($"Role with name '{dto.Name}' already exists.");
        }

        var role = new Role
        {
            Name = dto.Name,
            Description = dto.Description,
            IsSystemRole = false,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        // Assign permissions if provided
        if (dto.PermissionIds != null && dto.PermissionIds.Count > 0)
        {
            foreach (var permissionId in dto.PermissionIds)
            {
                var permissionExists = await _context.Permissions.AnyAsync(p => p.Id == permissionId);
                if (permissionExists)
                {
                    var rolePermission = new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permissionId
                    };
                    _context.RolePermissions.Add(rolePermission);
                }
            }
            await _context.SaveChangesAsync();
        }

        // Load related entities for response
        await _context.Entry(role).Reference(r => r.Company).LoadAsync();
        await _context.Entry(role).Collection(r => r.RolePermissions).LoadAsync();
        foreach (var rp in role.RolePermissions)
        {
            await _context.Entry(rp).Reference(rp => rp.Permission).LoadAsync();
        }
        await _context.Entry(role).Collection(r => r.UserRoles).LoadAsync();

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            CompanyId = role.CompanyId,
            CompanyName = role.Company != null ? role.Company.Name : null,
            UserCount = 0,
            Permissions = role.RolePermissions.Select(rp => rp.Permission.Name).ToList(),
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        };
    }

    public async Task<RoleDto?> UpdateRoleAsync(int id, UpdateRoleDto dto)
    {
        var role = await _context.Roles
            .Include(r => r.Company)
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null) return null;

        if (role.IsSystemRole)
        {
            throw new InvalidOperationException("Cannot modify system roles.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            // Check if name already exists
            var existingRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == dto.Name && r.Id != id && r.CompanyId == role.CompanyId);
            if (existingRole != null)
            {
                throw new InvalidOperationException($"Role with name '{dto.Name}' already exists.");
            }
            role.Name = dto.Name;
        }

        if (dto.Description != null)
        {
            role.Description = dto.Description;
        }

        role.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            CompanyId = role.CompanyId,
            CompanyName = role.Company != null ? role.Company.Name : null,
            UserCount = role.UserRoles.Count(ur => ur.IsActive),
            Permissions = role.RolePermissions.Select(rp => rp.Permission.Name).ToList(),
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        };
    }

    public async Task<bool> DeleteRoleAsync(int id)
    {
        var role = await _context.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null) return false;

        if (role.IsSystemRole)
        {
            throw new InvalidOperationException("Cannot delete system roles.");
        }

        if (role.UserRoles.Any(ur => ur.IsActive))
        {
            throw new InvalidOperationException("Cannot delete role that is assigned to users.");
        }

        // Remove all role permissions
        var rolePermissions = await _context.RolePermissions.Where(rp => rp.RoleId == id).ToListAsync();
        _context.RolePermissions.RemoveRange(rolePermissions);

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null) return false;

        var permission = await _context.Permissions.FindAsync(permissionId);
        if (permission == null) return false;

        // Check if already assigned
        var existing = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
        if (existing != null) return true;

        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };

        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (rolePermission == null) return false;

        _context.RolePermissions.Remove(rolePermission);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
    {
        var permissions = await _context.Permissions
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Module = p.Module
            })
            .ToListAsync();

        return permissions;
    }

    public async Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(int roleId)
    {
        var permissions = await _context.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => new PermissionDto
            {
                Id = rp.Permission.Id,
                Name = rp.Permission.Name,
                Description = rp.Permission.Description,
                Module = rp.Permission.Module
            })
            .ToListAsync();

        return permissions;
    }
}

