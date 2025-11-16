namespace crm_backend.Modules.Customer.DTOs;

public class UpdateCustomerDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Status { get; set; }

    // Additional Business Fields
    public string? ContactPersonName { get; set; }
    public string? CustomerType { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Priority { get; set; }

    // Sales Assignment
    public int? AssignedToTeamId { get; set; }
    public int? AssignedToUserId { get; set; }
}
