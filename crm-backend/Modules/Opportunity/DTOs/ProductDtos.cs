namespace crm_backend.Modules.Opportunity.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? Cost { get; set; }
    public ProductType Type { get; set; }
    public string? Unit { get; set; }
    public bool IsActive { get; set; }
    public bool IsTaxable { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? Cost { get; set; }
    public ProductType Type { get; set; } = ProductType.Product;
    public string? Unit { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsTaxable { get; set; } = true;
    public int? CompanyId { get; set; } // Optional, will be set from authenticated user
}

public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public decimal? Cost { get; set; }
    public ProductType? Type { get; set; }
    public string? Unit { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsTaxable { get; set; }
}

