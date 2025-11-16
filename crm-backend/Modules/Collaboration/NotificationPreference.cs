namespace crm_backend.Modules.Collaboration;

public class NotificationPreference
{
    public int Id { get; set; }

    // Relationships
    public int UserId { get; set; }
    public User.User User { get; set; } = null!;

    // Preference Configuration
    public NotificationType NotificationType { get; set; }
    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
    public bool IsEnabled { get; set; } = true;

    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum NotificationChannel
{
    InApp,  // In-app notification
    Email,  // Email notification
    Push,   // Push notification (mobile)
    SMS     // SMS notification
}

