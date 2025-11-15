using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.Services;

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllRolesAsync(int? companyId = null);
    Task<RoleDto?> GetRoleByIdAsync(int id);
    Task<RoleDto> CreateRoleAsync(CreateRoleDto dto);
    Task<RoleDto?> UpdateRoleAsync(int id, UpdateRoleDto dto);
    Task<bool> DeleteRoleAsync(int id);
    Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId);
    Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId);
    Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
    Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(int roleId);
}

