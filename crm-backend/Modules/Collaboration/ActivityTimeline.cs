using System.Text.Json;

namespace crm_backend.Modules.Collaboration;

public class ActivityTimeline
{
    public int Id { get; set; }
    
    // Entity Reference (polymorphic)
    public string EntityType { get; set; } = string.Empty; // Customer, Opportunity, Lead, Channel, etc.
    public int EntityId { get; set; }
    
    // Activity Details
    public ActivityType Type { get; set; } = ActivityType.Note;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Performed By
    public int? PerformedByUserId { get; set; } // Nullable for AI actions
    public User.User? PerformedByUser { get; set; }
    
    public int? AIAgentId { get; set; } // If AI action
    public AIAgent? AIAgent { get; set; }
    
    // Metadata (JSON) - stores additional context like old/new values, related entities, etc.
    public string? Metadata { get; set; }
    
    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public ICollection<ActivityFeed> ActivityFeeds { get; set; } = new List<ActivityFeed>();
}

public enum ActivityType
{
    Note,
    Call,
    Email,
    Meeting,
    StatusChange,
    AIAction,
    Strategy,
    Idea,
    Message,
    FileUpload,
    TaskCreated,
    TaskCompleted,
    OpportunityCreated,
    OpportunityWon,
    OpportunityLost,
    QuoteSent,
    InvoiceCreated,
    PaymentReceived,
    ContractSigned
}

