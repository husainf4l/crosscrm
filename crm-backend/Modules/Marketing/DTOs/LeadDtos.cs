using crm_backend.Modules.Marketing;

namespace crm_backend.Modules.Marketing.DTOs;

public class LeadDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Industry { get; set; }
    public decimal? EstimatedValue { get; set; }
    public string? Currency { get; set; }
    public LeadStatus Status { get; set; }
    public LeadRating Rating { get; set; }
    public int? SourceId { get; set; }
    public string? SourceName { get; set; }
    public int? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public int? ConvertedToCustomerId { get; set; }
    public string? ConvertedToCustomerName { get; set; }
    public int? ConvertedToOpportunityId { get; set; }
    public string? ConvertedToOpportunityName { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public int? ConvertedByUserId { get; set; }
    public string? ConvertedByUserName { get; set; }
    public int CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateLeadDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Industry { get; set; }
    public decimal? EstimatedValue { get; set; }
    public string? Currency { get; set; } = "USD";
    public LeadRating Rating { get; set; } = LeadRating.Cold;
    public int? SourceId { get; set; }
    public int? AssignedUserId { get; set; }
    public int? CompanyId { get; set; } // Optional, will be set from authenticated user
}

public class UpdateLeadDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CompanyName { get; set; }
    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Industry { get; set; }
    public decimal? EstimatedValue { get; set; }
    public string? Currency { get; set; }
    public LeadStatus? Status { get; set; }
    public LeadRating? Rating { get; set; }
    public int? SourceId { get; set; }
    public int? AssignedUserId { get; set; }
}

public class ConvertLeadDto
{
    public bool CreateCustomer { get; set; } = true;
    public int? CustomerId { get; set; } // Link to existing customer instead of creating
    public bool CreateOpportunity { get; set; } = true;
    public int? OpportunityId { get; set; } // Link to existing opportunity instead of creating
    public string? CustomerName { get; set; } // If different from lead's company name
}

