using System.ComponentModel.DataAnnotations;

namespace crm_backend.Modules.User;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    [StringLength(500)]
    public string? Avatar { get; set; }

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Sales Hierarchy
    public int? ManagerId { get; set; }
    public User? Manager { get; set; }
    public ICollection<User> DirectReports { get; set; } = new List<User>();

    // Foreign key for active company
    public int? CompanyId { get; set; }

    // Navigation property for active company
    public crm_backend.Modules.Company.Company? Company { get; set; }

    // Many-to-many relationship with companies
    public ICollection<crm_backend.Modules.User.UserCompany> UserCompanies { get; set; } = new List<crm_backend.Modules.User.UserCompany>();

    // Team memberships
    public ICollection<crm_backend.Modules.Collaboration.TeamMember> TeamMemberships { get; set; } = new List<crm_backend.Modules.Collaboration.TeamMember>();

    // User roles
    public ICollection<crm_backend.Modules.Collaboration.UserRole> UserRoles { get; set; } = new List<crm_backend.Modules.Collaboration.UserRole>();
}