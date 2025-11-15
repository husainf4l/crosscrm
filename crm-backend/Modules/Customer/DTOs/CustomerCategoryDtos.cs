using System.ComponentModel.DataAnnotations;

namespace crm_backend.Modules.Customer.DTOs;

public class CustomerCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CustomerCount { get; set; }
}

public class CreateCustomerCategoryDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(7)]
    public string? Color { get; set; }
}

public class UpdateCustomerCategoryDto
{
    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(7)]
    public string? Color { get; set; }

    public bool? IsActive { get; set; }
}

public class CustomerCategoryMappingDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryColor { get; set; }
    public int AssignedByUserId { get; set; }
    public string AssignedByUserName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public string? Notes { get; set; }
}

public class AssignCustomerToCategoryDto
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}