namespace crm_backend.Modules.Marketing;

public class Campaign
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Campaign Details
    public CampaignType Type { get; set; } = CampaignType.Email;
    public CampaignStatus Status { get; set; } = CampaignStatus.Planned;

    // Dates
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Budget & Costs
    public decimal? Budget { get; set; }
    public decimal? ActualCost { get; set; }
    public string Currency { get; set; } = "USD";

    // Metrics
    public int? ExpectedLeads { get; set; }
    public int ActualLeads { get; set; } = 0;

    public decimal? ExpectedRevenue { get; set; }
    public decimal ActualRevenue { get; set; } = 0;

    // Relationships
    public int CreatedByUserId { get; set; }
    public User.User CreatedByUser { get; set; } = null!;

    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<CampaignMember> Members { get; set; } = new List<CampaignMember>();
}

public enum CampaignType
{
    Email,
    SocialMedia,
    Print,
    Webinar,
    TradeShow,
    Referral,
    Other
}

public enum CampaignStatus
{
    Planned,
    Active,
    Completed,
    Cancelled
}

