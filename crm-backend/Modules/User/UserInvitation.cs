using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace crm_backend.Modules.User;

public class UserInvitation
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string InvitationToken { get; set; } = string.Empty;

    [Required]
    public int CompanyId { get; set; }

    [Required]
    public int InvitedByUserId { get; set; }

    public int? RoleId { get; set; }

    public int? TeamId { get; set; }

    [Required]
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime ExpiresAt { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public int? AcceptedByUserId { get; set; }

    public string? Notes { get; set; }

    // Navigation properties
    [ForeignKey("CompanyId")]
    public virtual crm_backend.Modules.Company.Company Company { get; set; } = null!;

    [ForeignKey("InvitedByUserId")]
    public virtual User InvitedByUser { get; set; } = null!;

    [ForeignKey("AcceptedByUserId")]
    public virtual User? AcceptedByUser { get; set; }

    [ForeignKey("RoleId")]
    public virtual crm_backend.Modules.Collaboration.Role? Role { get; set; }

    [ForeignKey("TeamId")]
    public virtual crm_backend.Modules.Collaboration.Team? Team { get; set; }
}

public enum InvitationStatus
{
    Pending = 0,
    Accepted = 1,
    Declined = 2,
    Expired = 3,
    Cancelled = 4
}
