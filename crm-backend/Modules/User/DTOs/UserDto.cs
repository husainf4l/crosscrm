namespace crm_backend.Modules.User.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Avatar { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int? ManagerId { get; set; }
    public string? Role { get; set; } // Primary role in current company
    public string Status { get; set; } = "Active"; // User status (Active, Inactive, etc.)
    public DateTime? JoinedAt { get; set; } // When user joined the current company
    public DateTime? LastLoginAt { get; set; } // Last login timestamp
    public UserBasicDto? Manager { get; set; }
    public List<UserBasicDto> DirectReports { get; set; } = new List<UserBasicDto>();
    public List<UserCompanyDto> Companies { get; set; } = new List<UserCompanyDto>();
    public List<TeamMembershipDto> Teams { get; set; } = new List<TeamMembershipDto>();
    public List<UserRoleAssignmentDto> Roles { get; set; } = new List<UserRoleAssignmentDto>();
}

public class UserBasicDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; }
}

public class TeamMembershipDto
{
    public int Id { get; set; } // TeamMember ID
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty; // User's name in this team context
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class UserRoleAssignmentDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime AssignedAt { get; set; }
}
