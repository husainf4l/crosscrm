using System.ComponentModel.DataAnnotations;

namespace crm_backend.Modules.Customer.DTOs;

public class CustomerPreferenceDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string PreferenceKey { get; set; } = string.Empty;
    public string? PreferenceValue { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? SetByUserId { get; set; }
    public string? SetByUserName { get; set; }
}

public class CreateCustomerPreferenceDto
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    [StringLength(100)]
    public string PreferenceKey { get; set; } = string.Empty;

    [StringLength(500)]
    public string? PreferenceValue { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }
}

public class UpdateCustomerPreferenceDto
{
    [StringLength(500)]
    public string? PreferenceValue { get; set; }

    public bool? IsActive { get; set; }
}
