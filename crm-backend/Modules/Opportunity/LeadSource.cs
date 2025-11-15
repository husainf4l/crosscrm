namespace crm_backend.Modules.Opportunity;

public class LeadSource
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // "Website", "Referral", "Cold Call", "Trade Show", etc.
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
    
    public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
}

