namespace crm_backend.Modules.Opportunity;

/// <summary>
/// PriceBook defines pricing for products (Standard, Volume, Contract pricing)
/// </summary>
public class PriceBook
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PriceBookType Type { get; set; } = PriceBookType.Standard;
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false; // Default price book for company

    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    // Navigation
    public ICollection<PriceBookEntry> Entries { get; set; } = new List<PriceBookEntry>();
}

public enum PriceBookType
{
    Standard,
    Volume,
    Contract,
    Promotional,
    Custom
}

/// <summary>
/// PriceBookEntry defines the price for a product in a specific price book
/// </summary>
public class PriceBookEntry
{
    public int Id { get; set; }
    public int PriceBookId { get; set; }
    public PriceBook PriceBook { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    // Pricing
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "USD";

    // Discount Rules
    public DiscountType? DiscountType { get; set; } // Percentage, Fixed, Tiered
    public decimal? DiscountValue { get; set; }
    public string? DiscountRules { get; set; } // JSON for tiered discounts: [{"minQty": 10, "discount": 5}, ...]

    // Minimum/Maximum quantities
    public int? MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    // Status
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Financial.QuoteLineItem> QuoteLineItems { get; set; } = new List<Financial.QuoteLineItem>();
}

public enum DiscountType
{
    Percentage,
    Fixed,
    Tiered
}

