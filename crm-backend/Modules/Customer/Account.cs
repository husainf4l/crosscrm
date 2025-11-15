namespace crm_backend.Modules.Customer;

/// <summary>
/// Account represents a Company/Organization (B2B model)
/// Customer represents the relationship/transaction with an Account
/// </summary>
public class Account
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public int? EmployeeCount { get; set; }
    
    // Addresses
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingPostalCode { get; set; }
    public string? BillingCountry { get; set; }
    
    public string? ShippingAddress { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ShippingCountry { get; set; }
    
    // Account Type
    public AccountType Type { get; set; } = AccountType.Customer;
    
    // Account Hierarchy
    public int? ParentAccountId { get; set; }
    public Account? ParentAccount { get; set; }
    public ICollection<Account> ChildAccounts { get; set; } = new List<Account>();
    
    // Assignment
    public int? AssignedToUserId { get; set; }
    public User.User? AssignedToUser { get; set; }
    
    public int? AssignedToTeamId { get; set; }
    public Collaboration.Team? AssignedTeam { get; set; }
    
    // TODO: Implement Territory module
    // public int? TerritoryId { get; set; }
    // public Territory.Territory? Territory { get; set; }
    
    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Status
    public string Status { get; set; } = "Active"; // Active, Inactive, Prospect
    
    // Navigation
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Opportunity.Opportunity> Opportunities { get; set; } = new List<Opportunity.Opportunity>();
    public ICollection<Contract.Contract> Contracts { get; set; } = new List<Contract.Contract>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}

public enum AccountType
{
    Customer,
    Partner,
    Competitor,
    Prospect,
    Vendor,
    Other
}

