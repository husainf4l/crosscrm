namespace crm_backend.Modules.Collaboration;

public class Message
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageContentType ContentType { get; set; } = MessageContentType.Text;

    // Relationships
    public int ChannelId { get; set; }
    public Channel Channel { get; set; } = null!;

    public int? CreatedByUserId { get; set; } // Nullable for AI agents
    public User.User? CreatedByUser { get; set; }

    public int? AIAgentId { get; set; } // If AI-generated
    public AIAgent? AIAgent { get; set; }

    // Threading
    public int? ParentMessageId { get; set; }
    public Message? ParentMessage { get; set; }

    // Status
    public bool IsEdited { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public DateTime? EditedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Reactions (JSON: {"👍": [userId1, userId2], "❤️": [userId3]})
    public string Reactions { get; set; } = "{}";

    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<Message> ThreadReplies { get; set; } = new List<Message>();
    public ICollection<MessageMention> Mentions { get; set; } = new List<MessageMention>();
    public ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
}

public enum MessageContentType
{
    Text,
    Markdown,
    RichText
}

