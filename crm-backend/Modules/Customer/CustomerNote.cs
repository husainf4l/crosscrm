namespace crm_backend.Modules.Customer;

public class CustomerNote
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NoteType Type { get; set; } = NoteType.General;

    // Relationships
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int CreatedByUserId { get; set; }
    public crm_backend.Modules.User.User CreatedByUser { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Additional fields
    public bool IsPrivate { get; set; } = false; // Private notes only visible to creator/company
    public bool IsPinned { get; set; } = false; // Pin important notes
    public string? Tags { get; set; } // JSON array of tags
    
    // Navigation to Comments (polymorphic via NoteComment)
}

public enum NoteType
{
    General,
    Call,
    Meeting,
    Email,
    FollowUp,
    Complaint,
    Internal,
    Strategy,
    Idea,
    AIInsight
}