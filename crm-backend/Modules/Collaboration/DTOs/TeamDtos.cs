using crm_backend.Modules.Collaboration;

namespace crm_backend.Modules.Collaboration.DTOs;

public class TeamDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TeamType Type { get; set; }
    public bool IsActive { get; set; }
    public int? ManagerUserId { get; set; }
    public string? ManagerName { get; set; }
    public ManagerBasicDto? Manager { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public List<TeamMemberDto>? Members { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ManagerBasicDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CreateTeamDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TeamType Type { get; set; } = TeamType.Sales;
    public int? ManagerUserId { get; set; }
    public int? CompanyId { get; set; } // Optional, will be set from authenticated user
    public List<int>? MemberUserIds { get; set; } // Optional initial members
}

public class UpdateTeamDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public TeamType? Type { get; set; }
    public bool? IsActive { get; set; }
    public int? ManagerUserId { get; set; }
}

public class TeamMemberDto
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public TeamMemberRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
}

public class AddTeamMemberDto
{
    public int TeamId { get; set; }
    public int UserId { get; set; }
    public TeamMemberRole Role { get; set; } = TeamMemberRole.Member;
}

public class UpdateTeamMemberDto
{
    public TeamMemberRole? Role { get; set; }
    public bool? IsActive { get; set; }
}

