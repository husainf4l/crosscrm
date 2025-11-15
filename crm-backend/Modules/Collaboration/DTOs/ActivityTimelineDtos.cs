using System.Text.Json;
using HotChocolate;

namespace crm_backend.Modules.Collaboration.DTOs;

public class ActivityTimelineDto
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? PerformedByUserId { get; set; }
    public string? PerformedByUserName { get; set; }
    public int? AIAgentId { get; set; }
    public string? AIAgentName { get; set; }
    
    [GraphQLIgnore]
    public Dictionary<string, object>? Metadata { get; set; }
    
    public string? MetadataJson => Metadata != null 
        ? JsonSerializer.Serialize(Metadata) 
        : null;
    
    public int CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; } // For user-specific feeds
}

public class CreateActivityTimelineDto
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Type { get; set; } = string.Empty; // ActivityType enum as string
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? AIAgentId { get; set; }
    
    [GraphQLIgnore]
    public Dictionary<string, object>? Metadata { get; set; }
    
    public string? MetadataJson
    {
        get => Metadata != null ? JsonSerializer.Serialize(Metadata) : null;
        set => Metadata = !string.IsNullOrEmpty(value) 
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(value) 
            : null;
    }
    
    public List<int>? NotifyUserIds { get; set; } // Optional: users to notify about this activity
}

public class ActivityFeedDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public int ActivityId { get; set; }
    public ActivityTimelineDto? Activity { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

