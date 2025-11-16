namespace crm_backend.Modules.Collaboration;

public class Notification
{
    public int Id { get; set; }

    // Relationships
    public int UserId { get; set; }
    public User.User User { get; set; } = null!;

    // Notification Content
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Activity;

    // Related Entity (polymorphic)
    public string? EntityType { get; set; } // e.g., "Customer", "Message", "Strategy", "Idea"
    public int? EntityId { get; set; }

    // Action
    public string? ActionUrl { get; set; } // URL to navigate to when clicked

    // Status
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    // Priority
    public NotificationPriority Priority { get; set; } = NotificationPriority.Medium;

    // Metadata
    public string? Metadata { get; set; } // JSON for additional data

    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum NotificationType
{
    Mention,           // User was mentioned in a message
    Message,           // New message in channel
    Activity,          // Activity on customer/entity
    AIAction,          // AI agent performed an action
    StrategyUpdate,    // Strategy was updated
    IdeaUpdate,        // Idea was updated
    TaskAssigned,      // Task was assigned
    TaskCompleted,     // Task was completed
    AppointmentReminder, // Appointment reminder
    SystemAlert        // System alert
}

public enum NotificationPriority
{
    Low,
    Medium,
    High,
    Urgent
}

