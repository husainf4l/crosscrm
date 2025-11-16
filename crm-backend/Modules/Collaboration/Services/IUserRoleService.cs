using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.Services;

public interface IUserRoleService
{
    Task<IEnumerable<UserRoleDto>> GetUserRolesAsync(int userId, int? companyId = null);
    Task<UserRoleDto> AssignUserRoleAsync(AssignUserRoleDto dto, int assignedByUserId);
    Task<bool> RemoveUserRoleAsync(int userRoleId);
    Task<bool> UserHasRoleAsync(int userId, string roleName, int companyId);
    Task<bool> UserHasPermissionAsync(int userId, string permissionName, int companyId);
    Task<IEnumerable<string>> GetUserPermissionsAsync(int userId, int companyId);
}

