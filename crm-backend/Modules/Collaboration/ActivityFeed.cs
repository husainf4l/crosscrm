namespace crm_backend.Modules.Collaboration;

public class ActivityFeed
{
    public int Id { get; set; }

    // Relationships
    public int UserId { get; set; }
    public User.User User { get; set; } = null!;

    public int ActivityId { get; set; }
    public ActivityTimeline Activity { get; set; } = null!;

    // Status
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

