namespace crm_backend.Modules.Collaboration;

public class CustomerIdea
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Idea Configuration
    public IdeaCategory Category { get; set; } = IdeaCategory.Product;
    public IdeaStatus Status { get; set; } = IdeaStatus.New;
    public IdeaPriority Priority { get; set; } = IdeaPriority.Medium;

    // Voting
    public int Upvotes { get; set; } = 0;
    public int Downvotes { get; set; } = 0;

    // Relationships
    public int CustomerId { get; set; }
    public Customer.Customer Customer { get; set; } = null!;

    public int? WorkspaceId { get; set; }
    public CustomerWorkspace? Workspace { get; set; }

    public int CreatedByUserId { get; set; }
    public User.User CreatedByUser { get; set; } = null!;

    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<NoteComment> Comments { get; set; } = new List<NoteComment>();
    public ICollection<IdeaVote> Votes { get; set; } = new List<IdeaVote>();
}

public enum IdeaCategory
{
    Product,
    Service,
    Process,
    Marketing
}

public enum IdeaStatus
{
    New,
    UnderReview,
    Approved,
    Rejected,
    Implemented
}

public enum IdeaPriority
{
    Low,
    Medium,
    High,
    Critical
}

public class IdeaVote
{
    public int Id { get; set; }
    public int IdeaId { get; set; }
    public CustomerIdea Idea { get; set; } = null!;

    public int UserId { get; set; }
    public User.User User { get; set; } = null!;

    public bool IsUpvote { get; set; } // true for upvote, false for downvote

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

