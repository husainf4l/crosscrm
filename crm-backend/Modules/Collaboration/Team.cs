namespace crm_backend.Modules.Collaboration;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Team Configuration
    public TeamType Type { get; set; } = TeamType.Sales;
    public bool IsActive { get; set; } = true;
    
    // Relationships
    public int? ManagerUserId { get; set; }
    public User.User? Manager { get; set; }
    
    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
}

public enum TeamType
{
    Sales,
    Support,
    Management,
    CrossFunctional
}

