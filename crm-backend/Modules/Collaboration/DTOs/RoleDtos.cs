using crm_backend.Modules.Collaboration;

namespace crm_backend.Modules.Collaboration.DTOs;

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int UserCount { get; set; }
    public List<string> Permissions { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CompanyId { get; set; } // Optional, will be set from authenticated user
    public List<int>? PermissionIds { get; set; } // Optional permissions to assign
}

public class UpdateRoleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class UserRoleDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int? AssignedByUserId { get; set; }
    public string? AssignedByName { get; set; }
    public DateTime AssignedAt { get; set; }
    public bool IsActive { get; set; }
}

public class AssignUserRoleDto
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public int CompanyId { get; set; }
}

public class PermissionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;
}

