namespace crm_backend.Modules.Financial;

public class Quote
{
    public int Id { get; set; }
    public string QuoteNumber { get; set; } = string.Empty; // Auto-generated: "QT-2025-001"
    public string? Title { get; set; }
    public string? Description { get; set; }

    // Financial
    public decimal SubTotal { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";

    // Status
    public QuoteStatus Status { get; set; } = QuoteStatus.Draft;
    public DateTime? ValidUntil { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? RejectedAt { get; set; }

    // Relationships
    public int CustomerId { get; set; }
    public Customer.Customer Customer { get; set; } = null!;

    public int? OpportunityId { get; set; }
    public Opportunity.Opportunity? Opportunity { get; set; }

    public int? AssignedToTeamId { get; set; }
    public Collaboration.Team? AssignedTeam { get; set; }

    public int? ConvertedToInvoiceId { get; set; }
    public Invoice? ConvertedToInvoice { get; set; }

    public int CreatedByUserId { get; set; }
    public User.User CreatedByUser { get; set; } = null!;

    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<QuoteLineItem> LineItems { get; set; } = new List<QuoteLineItem>();
    public ICollection<Customer.FileAttachment> Attachments { get; set; } = new List<Customer.FileAttachment>();
}

public enum QuoteStatus
{
    Draft,
    Sent,
    Accepted,
    Rejected,
    Expired,
    Converted // Converted to invoice
}

