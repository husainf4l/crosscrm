namespace crm_backend.Modules.Collaboration;

public class CustomerWorkspace
{
    public int Id { get; set; }

    // Relationships
    public int CustomerId { get; set; }
    public Customer.Customer Customer { get; set; } = null!;

    public int? TeamId { get; set; }
    public Team? Team { get; set; }

    // Workspace Content
    public string? Summary { get; set; } // AI-generated or manual summary

    // Tracking
    public DateTime? LastUpdatedAt { get; set; }
    public int? LastUpdatedByUserId { get; set; }
    public User.User? LastUpdatedByUser { get; set; }

    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<CustomerStrategy> Strategies { get; set; } = new List<CustomerStrategy>();
    public ICollection<CustomerIdea> Ideas { get; set; } = new List<CustomerIdea>();
}

