namespace crm_backend.Modules.Customer;

/// <summary>
/// Contact represents a person associated with an Account (B2B model)
/// Contact can also be linked to Customer for backward compatibility
/// </summary>
public class Contact
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Name => $"{FirstName} {LastName}".Trim();
    public string? Title { get; set; } // Job title/position
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public bool IsPrimary { get; set; } = false; // Primary contact person
    
    // Contact Role in the Account
    public ContactRole Role { get; set; } = ContactRole.Other;

    // Relationship with Account (B2B - primary relationship)
    public int? AccountId { get; set; }
    public Account? Account { get; set; }
    
    // Relationship with Customer (for backward compatibility)
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum ContactRole
{
    DecisionMaker,
    Influencer,
    User,
    TechnicalContact,
    BillingContact,
    Other
}