using System.ComponentModel.DataAnnotations;

namespace crm_backend.Modules.User.DTOs;

public class InviteUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public int CompanyId { get; set; }

    public int? RoleId { get; set; }

    public int? TeamId { get; set; }

    public string? Notes { get; set; }
}

public class UserInvitationDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string InvitationToken { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int InvitedByUserId { get; set; }
    public string InvitedByUserName { get; set; } = string.Empty;
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public InvitationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public int? AcceptedByUserId { get; set; }
    public string? AcceptedByUserName { get; set; }
    public string? Notes { get; set; }
}

public class AcceptInvitationDto
{
    [Required]
    public string InvitationToken { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    public string? Phone { get; set; }
}

public class InvitationResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserInvitationDto? Invitation { get; set; }
    public AuthResponseDto? AuthResponse { get; set; }
}
