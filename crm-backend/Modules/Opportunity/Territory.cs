namespace crm_backend.Modules.Opportunity;

/// <summary>
/// Territory represents a geographic or organizational sales territory
/// </summary>
public class Territory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    
    // Territory Manager
    public int? AssignedToUserId { get; set; }
    public User.User? AssignedToUser { get; set; }
    
    // Team Assignment
    public int? AssignedToTeamId { get; set; }
    public Collaboration.Team? AssignedTeam { get; set; }
    
    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Status
    public bool IsActive { get; set; } = true;
    
    // Navigation
    public ICollection<Customer.Customer> Customers { get; set; } = new List<Customer.Customer>();
    public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
    public ICollection<Customer.Account> Accounts { get; set; } = new List<Customer.Account>();
}

