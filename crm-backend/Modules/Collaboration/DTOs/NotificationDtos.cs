using HotChocolate;

namespace crm_backend.Modules.Collaboration.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string Priority { get; set; } = string.Empty;
    
    [GraphQLIgnore]
    public Dictionary<string, object>? Metadata { get; set; }
    
    public int CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateNotificationDto
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? ActionUrl { get; set; }
    public string Priority { get; set; } = "Medium";
    
    [GraphQLIgnore]
    public Dictionary<string, object>? Metadata { get; set; }
}

public class MarkNotificationReadDto
{
    public int NotificationId { get; set; }
}

public class MarkAllNotificationsReadDto
{
    // No properties needed - marks all for current user
}

public class NotificationPreferenceDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public int CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateNotificationPreferenceDto
{
    public string NotificationType { get; set; } = string.Empty;
    public string Channel { get; set; } = "InApp";
    public bool IsEnabled { get; set; } = true;
}

public class UpdateNotificationPreferenceDto
{
    public bool? IsEnabled { get; set; }
}

public class NotificationCountDto
{
    public int UnreadCount { get; set; }
    public int TotalCount { get; set; }
}

