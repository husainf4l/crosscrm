namespace crm_backend.Modules.Collaboration;

public class NoteComment
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;

    // Polymorphic relationship - can comment on Note, Strategy, or Idea
    public string EntityType { get; set; } = string.Empty; // "Note", "Strategy", "Idea"
    public int EntityId { get; set; }

    // Threading support
    public int? ParentCommentId { get; set; }
    public NoteComment? ParentComment { get; set; }

    // Relationships
    public int CreatedByUserId { get; set; }
    public User.User CreatedByUser { get; set; } = null!;

    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<NoteComment> Replies { get; set; } = new List<NoteComment>();
}

