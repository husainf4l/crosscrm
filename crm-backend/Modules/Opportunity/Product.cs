namespace crm_backend.Modules.Opportunity;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; } // Stock Keeping Unit
    
    // Pricing
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? Cost { get; set; } // Cost price for margin calculation
    
    // Product Type
    public ProductType Type { get; set; } = ProductType.Product; // Product, Service, Subscription
    public string? Unit { get; set; } // "each", "hour", "month", etc.
    
    // Product Attributes
    public string? Barcode { get; set; }
    public decimal? Weight { get; set; } // in kg
    public decimal? Length { get; set; } // in cm
    public decimal? Width { get; set; } // in cm
    public decimal? Height { get; set; } // in cm
    
    // Product Organization
    public int? CategoryId { get; set; }
    public ProductCategory? Category { get; set; }
    
    public int? FamilyId { get; set; }
    public ProductFamily? Family { get; set; }
    
    // Product Variants (for Size, Color, etc.)
    public string? VariantAttributes { get; set; } // JSON: {"Size": "Large", "Color": "Red"}
    
    // Product Images
    public string? ImageUrl { get; set; }
    public string? ImageUrls { get; set; } // JSON array of image URLs
    
    // Status
    public bool IsActive { get; set; } = true;
    public bool IsTaxable { get; set; } = true;
    
    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<OpportunityProduct> OpportunityProducts { get; set; } = new List<OpportunityProduct>();
    public ICollection<PriceBookEntry> PriceBookEntries { get; set; } = new List<PriceBookEntry>();
}

public enum ProductType
{
    Product,
    Service,
    Subscription
}

