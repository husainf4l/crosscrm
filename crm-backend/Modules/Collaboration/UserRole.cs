namespace crm_backend.Modules.Collaboration;

public class UserRole
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User.User User { get; set; } = null!;
    
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    
    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
    
    // Assignment tracking
    public int? AssignedByUserId { get; set; }
    public User.User? AssignedByUser { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

