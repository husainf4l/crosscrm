namespace crm_backend.Modules.Customer;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    
    // GPS Coordinates
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    // Relationship with Company
    public int CompanyId { get; set; }
    public crm_backend.Modules.Company.Company? Company { get; set; }
    
    public int? AssignedToTeamId { get; set; }
    public crm_backend.Modules.Collaboration.Team? AssignedTeam { get; set; }
    
    public int? ConvertedFromLeadId { get; set; }
    public crm_backend.Modules.Marketing.Lead? ConvertedFromLead { get; set; }
    
    // Territory Assignment
    public int? TerritoryId { get; set; }
    public Opportunity.Territory? Territory { get; set; }
    
    // Account Relationship (B2B)
    // Customer represents the relationship/transaction, Account is the company
    public int? AccountId { get; set; }
    public Account? Account { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Customer Status
    public string Status { get; set; } = "active"; // "active", "inactive", "prospect", "lost", etc.

    // Navigation properties for future relations
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<CustomerNote> Notes { get; set; } = new List<CustomerNote>();
    public ICollection<FileAttachment> FileAttachments { get; set; } = new List<FileAttachment>();

    // New relationships
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    public ICollection<CustomerPreference> Preferences { get; set; } = new List<CustomerPreference>();
    public ICollection<CustomerCategoryMapping> CategoryMappings { get; set; } = new List<CustomerCategoryMapping>();
}
