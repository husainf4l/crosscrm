namespace crm_backend.Modules.Customer.DTOs;

public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string Status { get; set; } = "active";
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Additional Business Fields
    public string? ContactPersonName { get; set; }
    public string? CustomerType { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Priority { get; set; }

    // Category information
    public List<CustomerCategoryDto> Categories { get; set; } = new List<CustomerCategoryDto>();

    // Activity summary
    public int TotalActivities { get; set; }
    public DateTime? LastActivity { get; set; }
}
