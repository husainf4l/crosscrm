namespace crm_backend.Modules.Collaboration;

public class AIAgentInteraction
{
    public int Id { get; set; }

    // Relationships
    public int AgentId { get; set; }
    public AIAgent Agent { get; set; } = null!;

    // Interaction Details
    public InteractionType Type { get; set; } = InteractionType.Query;

    // Entity Context (optional)
    public string? EntityType { get; set; } // Customer, Opportunity, Campaign, etc.
    public int? EntityId { get; set; }

    // Request/Response
    public string Input { get; set; } = string.Empty; // User query/request sent to Python service
    public string? Output { get; set; } // AI response from Python service
    public InteractionStatus Status { get; set; } = InteractionStatus.Pending;

    // Tracking
    public string? RequestId { get; set; } // Unique request ID for tracking with Python service
    public int? UserId { get; set; } // Who triggered the interaction
    public User.User? User { get; set; }

    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}

public enum InteractionType
{
    Query,
    Action,
    Analysis,
    Suggestion,
    Message
}

public enum InteractionStatus
{
    Pending,
    Processing,
    Success,
    Failed
}

