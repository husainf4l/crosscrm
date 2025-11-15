namespace crm_backend.Modules.Marketing;

public class Lead
{
    public int Id { get; set; }
    
    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Title { get; set; }
    
    // Contact Information
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Website { get; set; }
    
    // Address
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    
    // Business Information
    public string? Industry { get; set; }
    public decimal? EstimatedValue { get; set; }
    public string? Currency { get; set; } = "USD";
    
    // Lead Management
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public LeadRating Rating { get; set; } = LeadRating.Cold;
    public int? LeadScore { get; set; } // 0-100, calculated based on various factors
    
    // Relationships
    public int? SourceId { get; set; }
    public Opportunity.LeadSource? Source { get; set; }
    
    public int? AssignedUserId { get; set; }
    public User.User? AssignedUser { get; set; }
    
    // Conversion Tracking
    public int? ConvertedToCustomerId { get; set; }
    public Customer.Customer? ConvertedToCustomer { get; set; }
    
    public int? ConvertedToOpportunityId { get; set; }
    public Opportunity.Opportunity? ConvertedToOpportunity { get; set; }
    
    public DateTime? ConvertedAt { get; set; }
    public int? ConvertedByUserId { get; set; }
    public User.User? ConvertedByUser { get; set; }
    
    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<CampaignMember> CampaignMembers { get; set; } = new List<CampaignMember>();
}

public enum LeadStatus
{
    New,
    Contacted,
    Qualified,
    Converted,
    Lost,
    Unqualified
}

public enum LeadRating
{
    Hot,
    Warm,
    Cold
}

