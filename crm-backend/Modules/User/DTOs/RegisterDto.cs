using System.ComponentModel.DataAnnotations;

namespace crm_backend.Modules.User.DTOs;

public class RegisterDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    public int? CompanyId { get; set; }
}