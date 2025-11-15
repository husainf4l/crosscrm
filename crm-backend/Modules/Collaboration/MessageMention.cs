namespace crm_backend.Modules.Collaboration;

public class MessageMention
{
    public int Id { get; set; }
    
    // Relationships
    public int MessageId { get; set; }
    public Message Message { get; set; } = null!;
    
    // Mentioned entities (one of these will be set)
    public int? MentionedUserId { get; set; }
    public User.User? MentionedUser { get; set; }
    
    public int? MentionedTeamId { get; set; }
    public Team? MentionedTeam { get; set; }
    
    public int? MentionedAIAgentId { get; set; }
    public AIAgent? MentionedAIAgent { get; set; }
    
    // Status
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    
    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

