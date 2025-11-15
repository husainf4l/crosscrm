using System.Text.Json;

namespace crm_backend.Modules.Collaboration;

public class CustomerStrategy
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Strategy Configuration
    public StrategyType Type { get; set; } = StrategyType.Engagement;
    public StrategyStatus Status { get; set; } = StrategyStatus.Draft;
    public StrategyPriority Priority { get; set; } = StrategyPriority.Medium;
    
    // Dates
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Success Metrics (JSON)
    public string? SuccessMetrics { get; set; } // JSON object with metrics
    
    // Relationships
    public int CustomerId { get; set; }
    public Customer.Customer Customer { get; set; } = null!;
    
    public int? WorkspaceId { get; set; }
    public CustomerWorkspace? Workspace { get; set; }
    
    public int CreatedByUserId { get; set; }
    public User.User CreatedByUser { get; set; } = null!;
    
    public int? AssignedToTeamId { get; set; }
    public Team? AssignedTeam { get; set; }
    
    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<NoteComment> Comments { get; set; } = new List<NoteComment>();
}

public enum StrategyType
{
    Engagement,
    Upsell,
    Retention,
    WinBack
}

public enum StrategyStatus
{
    Draft,
    Active,
    Completed,
    Cancelled
}

public enum StrategyPriority
{
    Low,
    Medium,
    High,
    Critical
}

