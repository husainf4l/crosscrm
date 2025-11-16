using System.ComponentModel.DataAnnotations;

namespace crm_backend.Modules.Customer;

public class CustomerCategoryMapping
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int CategoryId { get; set; }
    public CustomerCategory Category { get; set; } = null!;

    // Who assigned this category
    public int AssignedByUserId { get; set; }
    public crm_backend.Modules.User.User AssignedByUser { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? Notes { get; set; } // Optional notes about why this category was assigned
}
