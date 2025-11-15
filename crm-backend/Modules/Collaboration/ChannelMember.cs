using System.Text.Json;

namespace crm_backend.Modules.Collaboration;

public class ChannelMember
{
    public int Id { get; set; }
    
    // Relationships
    public int ChannelId { get; set; }
    public Channel Channel { get; set; } = null!;
    
    public int UserId { get; set; }
    public User.User User { get; set; } = null!;
    
    // Member Configuration
    public ChannelMemberRole Role { get; set; } = ChannelMemberRole.Member;
    
    // Tracking
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastReadAt { get; set; }
    
    // Notification Settings (JSON)
    public string NotificationSettings { get; set; } = "{}"; // JSON object with notification preferences
}

public enum ChannelMemberRole
{
    Admin,
    Member,
    ReadOnly
}

