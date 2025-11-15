using System.ComponentModel.DataAnnotations;

namespace crm_backend.Modules.Customer;

public class CustomerCategory
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(7)]
    public string? Color { get; set; } // Hex color code for UI display

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Many-to-many relationship with customers
    public ICollection<CustomerCategoryMapping> CustomerMappings { get; set; } = new List<CustomerCategoryMapping>();
}