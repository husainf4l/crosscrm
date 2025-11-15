namespace crm_backend.Modules.Opportunity;

public class PipelineStage
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; } // Stage order in pipeline
    public decimal DefaultProbability { get; set; } // Default probability for this stage (0-100)
    public bool IsActive { get; set; } = true;
    public bool IsWonStage { get; set; } = false;
    public bool IsLostStage { get; set; } = false;
    
    // Company-specific pipeline
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
    
    public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
}

