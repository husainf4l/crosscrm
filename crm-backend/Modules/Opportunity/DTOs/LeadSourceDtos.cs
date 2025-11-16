namespace crm_backend.Modules.Opportunity.DTOs;

public class LeadSourceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
}

public class CreateLeadSourceDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int? CompanyId { get; set; } // Optional, will be set from authenticated user
}

public class UpdateLeadSourceDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

