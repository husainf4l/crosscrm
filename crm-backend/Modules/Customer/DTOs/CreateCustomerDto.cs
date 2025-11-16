namespace crm_backend.Modules.Customer.DTOs;

public class CreateCustomerDto
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? CompanyId { get; set; } // Made optional since it's set automatically from authenticated user
    
    // Additional Business Fields
    public string? ContactPersonName { get; set; }
    public string? CustomerType { get; set; } = "individual";
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Priority { get; set; } = "medium";
}
