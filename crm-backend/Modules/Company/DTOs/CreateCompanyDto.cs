using System.ComponentModel.DataAnnotations;

namespace crm_backend.Modules.Company.DTOs;

public class CreateCompanyDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }

    [StringLength(500)]
    public string? Logo { get; set; }

    [StringLength(255)]
    public string? Website { get; set; }

    [StringLength(100)]
    public string? Industry { get; set; }

    [StringLength(50)]
    public string? Size { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? State { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }
}
