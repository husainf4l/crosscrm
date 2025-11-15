using System.Text.Json;
using HotChocolate;

namespace crm_backend.Modules.Collaboration.DTOs;

public class ChannelDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public int CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int MemberCount { get; set; }
    public int UnreadCount { get; set; }
    public DateTime? LastMessageAt { get; set; }
}

public class CreateChannelDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty; // ChannelType enum as string
    public int? TeamId { get; set; }
    public int? CustomerId { get; set; }
    public List<int>? MemberUserIds { get; set; }
}

public class UpdateChannelDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsArchived { get; set; }
}

public class ChannelMemberDto
{
    public int Id { get; set; }
    public int ChannelId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public DateTime? LastReadAt { get; set; }
    
    [GraphQLIgnore]
    public Dictionary<string, object>? NotificationSettings { get; set; }
    
    public string? NotificationSettingsJson => NotificationSettings != null 
        ? JsonSerializer.Serialize(NotificationSettings) 
        : null;
}

public class AddChannelMemberDto
{
    public int ChannelId { get; set; }
    public int UserId { get; set; }
    public string? Role { get; set; }
}

public class MessageDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int ChannelId { get; set; }
    public string? ChannelName { get; set; }
    public int? CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public int? AIAgentId { get; set; }
    public string? AIAgentName { get; set; }
    public int? ParentMessageId { get; set; }
    public bool IsEdited { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? EditedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    [GraphQLIgnore]
    public Dictionary<string, List<int>>? Reactions { get; set; }
    
    public string? ReactionsJson => Reactions != null 
        ? JsonSerializer.Serialize(Reactions) 
        : null;
    
    public int CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ThreadReplyCount { get; set; }
    public int AttachmentCount { get; set; }
    public List<MessageMentionDto>? Mentions { get; set; }
    public List<MessageAttachmentDto>? Attachments { get; set; }
}

public class CreateMessageDto
{
    public int ChannelId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = "Text"; // MessageContentType enum as string
    public int? ParentMessageId { get; set; }
    public int? AIAgentId { get; set; }
    public List<int>? MentionedUserIds { get; set; }
    public List<int>? MentionedTeamIds { get; set; }
    public List<int>? MentionedAIAgentIds { get; set; }
}

public class UpdateMessageDto
{
    public string Content { get; set; } = string.Empty;
}

public class AddReactionDto
{
    public int MessageId { get; set; }
    public string Reaction { get; set; } = string.Empty; // emoji like "👍", "❤️", etc.
}

public class RemoveReactionDto
{
    public int MessageId { get; set; }
    public string Reaction { get; set; } = string.Empty;
}

public class MessageMentionDto
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public int? MentionedUserId { get; set; }
    public string? MentionedUserName { get; set; }
    public int? MentionedTeamId { get; set; }
    public string? MentionedTeamName { get; set; }
    public int? MentionedAIAgentId { get; set; }
    public string? MentionedAIAgentName { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MessageAttachmentDto
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int UploadedByUserId { get; set; }
    public string? UploadedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MarkChannelReadDto
{
    public int ChannelId { get; set; }
}

