namespace crm_backend.Modules.Collaboration;

public class AIAgentAssignment
{
    public int Id { get; set; }
    
    // Relationships
    public int AgentId { get; set; }
    public AIAgent Agent { get; set; } = null!;
    
    // Entity Assignment (polymorphic)
    public string EntityType { get; set; } = string.Empty; // Customer, Opportunity, Campaign, Team, Channel, etc.
    public int EntityId { get; set; }
    
    // Assignment Details
    public int AssignedByUserId { get; set; }
    public User.User AssignedByUser { get; set; } = null!;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    
    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

