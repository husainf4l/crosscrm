namespace crm_backend.Modules.Opportunity;

public class Opportunity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Financial
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal Probability { get; set; } // 0-100%
    public decimal WeightedAmount => Amount * (Probability / 100);

    // Pipeline
    public int PipelineStageId { get; set; }
    public PipelineStage PipelineStage { get; set; } = null!;
    public DateTime? ExpectedCloseDate { get; set; }
    public DateTime? ActualCloseDate { get; set; }

    // Relationships
    public int CustomerId { get; set; }
    public Customer.Customer Customer { get; set; } = null!;

    public int? AssignedUserId { get; set; }
    public User.User? AssignedUser { get; set; }

    public int? AssignedToTeamId { get; set; }
    public Collaboration.Team? AssignedTeam { get; set; }

    public int? SourceId { get; set; }
    public LeadSource? Source { get; set; }

    public int? ConvertedFromLeadId { get; set; }
    public Marketing.Lead? ConvertedFromLead { get; set; }

    // Territory Assignment
    public int? TerritoryId { get; set; }
    public Territory? Territory { get; set; }

    // Account Relationship (B2B)
    public int? AccountId { get; set; }
    public Customer.Account? Account { get; set; }

    // Status
    public OpportunityStatus Status { get; set; } = OpportunityStatus.Open;
    public string? LostReason { get; set; }
    public string? WinReason { get; set; }

    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? WonAt { get; set; }
    public DateTime? LostAt { get; set; }

    // Navigation
    public ICollection<Customer.ActivityLog> Activities { get; set; } = new List<Customer.ActivityLog>();
    public ICollection<Customer.FileAttachment> Attachments { get; set; } = new List<Customer.FileAttachment>();
    public ICollection<OpportunityProduct> Products { get; set; } = new List<OpportunityProduct>();
    public ICollection<Financial.Quote> Quotes { get; set; } = new List<Financial.Quote>();
    public ICollection<Contract.Contract> Contracts { get; set; } = new List<Contract.Contract>();
}

public enum OpportunityStatus
{
    Open,
    Won,
    Lost,
    Abandoned
}

