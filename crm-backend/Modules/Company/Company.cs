using System.ComponentModel.DataAnnotations;

namespace crm_backend.Modules.Company;

public class Company
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Many-to-many relationship with users
    public ICollection<crm_backend.Modules.User.UserCompany> UserCompanies { get; set; } = new List<crm_backend.Modules.User.UserCompany>();
}