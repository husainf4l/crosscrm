using System.ComponentModel.DataAnnotations;

namespace crm_backend.Modules.User.DTOs;

public class CreateUserDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    [Phone]
    public string? Phone { get; set; }

    public int? CompanyId { get; set; }
}