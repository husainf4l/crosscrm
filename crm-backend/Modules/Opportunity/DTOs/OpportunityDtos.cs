namespace crm_backend.Modules.Opportunity.DTOs;

public class OpportunityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal Probability { get; set; }
    public decimal WeightedAmount { get; set; }
    public int PipelineStageId { get; set; }
    public string PipelineStageName { get; set; } = string.Empty;
    public DateTime? ExpectedCloseDate { get; set; }
    public DateTime? ActualCloseDate { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public int? SourceId { get; set; }
    public string? SourceName { get; set; }
    public OpportunityStatus Status { get; set; }
    public string? LostReason { get; set; }
    public string? WinReason { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? WonAt { get; set; }
    public DateTime? LostAt { get; set; }
}

public class CreateOpportunityDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal Probability { get; set; }
    public int PipelineStageId { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public int CustomerId { get; set; }
    public int? AssignedUserId { get; set; }
    public int? SourceId { get; set; }
    public int? CompanyId { get; set; } // Optional, will be set from authenticated user
}

public class UpdateOpportunityDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public decimal? Probability { get; set; }
    public int? PipelineStageId { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public int? AssignedUserId { get; set; }
    public int? SourceId { get; set; }
    public OpportunityStatus? Status { get; set; }
    public string? LostReason { get; set; }
    public string? WinReason { get; set; }
}

