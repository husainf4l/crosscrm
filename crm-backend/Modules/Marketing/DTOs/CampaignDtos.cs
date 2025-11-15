using crm_backend.Modules.Marketing;

namespace crm_backend.Modules.Marketing.DTOs;

public class CampaignDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CampaignType Type { get; set; }
    public CampaignStatus Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public decimal? ActualCost { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int? ExpectedLeads { get; set; }
    public int ActualLeads { get; set; }
    public decimal? ExpectedRevenue { get; set; }
    public decimal ActualRevenue { get; set; }
    public int CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateCampaignDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CampaignType Type { get; set; } = CampaignType.Email;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public string Currency { get; set; } = "USD";
    public int? ExpectedLeads { get; set; }
    public decimal? ExpectedRevenue { get; set; }
    public int? CompanyId { get; set; } // Optional, will be set from authenticated user
    public int? CreatedByUserId { get; set; } // Optional, will be set from authenticated user
}

public class UpdateCampaignDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public CampaignType? Type { get; set; }
    public CampaignStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public decimal? ActualCost { get; set; }
    public string? Currency { get; set; }
    public int? ExpectedLeads { get; set; }
    public decimal? ExpectedRevenue { get; set; }
}

public class CampaignMemberDto
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public int? LeadId { get; set; }
    public string? LeadName { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public CampaignMemberStatus Status { get; set; }
    public DateTime? RespondedAt { get; set; }
    public DateTime? ConvertedAt { get; set; }
}

public class CreateCampaignMemberDto
{
    public int CampaignId { get; set; }
    public int? LeadId { get; set; }
    public int? CustomerId { get; set; }
    public int? ContactId { get; set; }
}

