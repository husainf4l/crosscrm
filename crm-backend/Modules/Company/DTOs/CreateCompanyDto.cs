using System.ComponentModel.DataAnnotations;

namespace crm_backend.Modules.Company.DTOs;

public class CreateCompanyDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}
