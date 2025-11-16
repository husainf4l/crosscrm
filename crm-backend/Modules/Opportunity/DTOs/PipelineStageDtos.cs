namespace crm_backend.Modules.Opportunity.DTOs;

public class PipelineStageDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public decimal DefaultProbability { get; set; }
    public bool IsActive { get; set; }
    public bool IsWonStage { get; set; }
    public bool IsLostStage { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
}

public class CreatePipelineStageDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public decimal DefaultProbability { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsWonStage { get; set; } = false;
    public bool IsLostStage { get; set; } = false;
    public int? CompanyId { get; set; } // Optional, will be set from authenticated user
}

public class UpdatePipelineStageDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? Order { get; set; }
    public decimal? DefaultProbability { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsWonStage { get; set; }
    public bool? IsLostStage { get; set; }
}

