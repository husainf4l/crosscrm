namespace crm_backend.Modules.Communication;

public class Email
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? BodyHtml { get; set; }
    
    // Email Details
    public string FromEmail { get; set; } = string.Empty;
    public string ToEmail { get; set; } = string.Empty;
    public string? CcEmail { get; set; }
    public string? BccEmail { get; set; }
    
    // Status
    public EmailDirection Direction { get; set; } // Inbound, Outbound
    public EmailStatus Status { get; set; } = EmailStatus.Draft;
    public DateTime? SentAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public bool IsRead { get; set; } = false;
    
    // Threading
    public string? ThreadId { get; set; } // Email thread ID
    public int? ParentEmailId { get; set; }
    public Email? ParentEmail { get; set; }
    
    // Relationships
    public int? CustomerId { get; set; }
    public Customer.Customer? Customer { get; set; }
    
    public int? ContactId { get; set; }
    public Customer.Contact? Contact { get; set; }
    
    public int? OpportunityId { get; set; }
    public Opportunity.Opportunity? Opportunity { get; set; }
    
    public int? QuoteId { get; set; }
    public Financial.Quote? Quote { get; set; }
    
    public int? InvoiceId { get; set; }
    public Financial.Invoice? Invoice { get; set; }
    
    public int? TicketId { get; set; }
    public Customer.Ticket? Ticket { get; set; }
    
    public int? SentByUserId { get; set; }
    public User.User? SentByUser { get; set; }
    
    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public ICollection<Customer.FileAttachment> Attachments { get; set; } = new List<Customer.FileAttachment>();
    public ICollection<Email> Replies { get; set; } = new List<Email>();
}

public enum EmailDirection
{
    Inbound,
    Outbound
}

public enum EmailStatus
{
    Draft,
    Sent,
    Received,
    Failed,
    Bounced
}

