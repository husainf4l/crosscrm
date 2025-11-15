namespace crm_backend.Modules.Contract;

public class Contract
{
    public int Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty; // Auto-generated
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Dates
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    public bool AutoRenew { get; set; } = false;
    
    // Financial
    public decimal TotalValue { get; set; }
    public string Currency { get; set; } = "USD";
    
    // Status
    public ContractStatus Status { get; set; } = ContractStatus.Draft;
    public DateTime? SignedAt { get; set; }
    
    // Relationships
    public int CustomerId { get; set; }
    public Customer.Customer Customer { get; set; } = null!;
    
    // Account Relationship (B2B)
    public int? AccountId { get; set; }
    public Customer.Account? Account { get; set; }
    
    public int? OpportunityId { get; set; }
    public Opportunity.Opportunity? Opportunity { get; set; }
    
    public int? InvoiceId { get; set; }
    public Financial.Invoice? Invoice { get; set; }
    
    public int CreatedByUserId { get; set; }
    public User.User CreatedByUser { get; set; } = null!;
    
    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<ContractLineItem> LineItems { get; set; } = new List<ContractLineItem>();
}

public enum ContractStatus
{
    Draft,
    Sent,
    Signed,
    Active,
    Expired,
    Cancelled,
    Renewed
}

