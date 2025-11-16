using System.ComponentModel.DataAnnotations;

namespace crm_backend.Modules.User.DTOs;

public class UpdateUserDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    [Phone]
    public string? Phone { get; set; }

    [StringLength(500)]
    public string? Avatar { get; set; }

    public int? ManagerId { get; set; }

    [Required]
    public int CompanyId { get; set; }
}