namespace crm_backend.Modules.Financial.DTOs;

public class QuoteDto
{
    public int Id { get; set; }
    public string QuoteNumber { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal SubTotal { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public QuoteStatus Status { get; set; }
    public DateTime? ValidUntil { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? OpportunityId { get; set; }
    public string? OpportunityName { get; set; }
    public int CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateQuoteDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime? ValidUntil { get; set; }
    public int CustomerId { get; set; }
    public int? OpportunityId { get; set; }
    public int? CompanyId { get; set; } // Optional, will be set from authenticated user
    public int? CreatedByUserId { get; set; } // Optional, will be set from authenticated user
    public List<CreateQuoteLineItemDto> LineItems { get; set; } = new List<CreateQuoteLineItemDto>();
}

public class UpdateQuoteDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string? Currency { get; set; }
    public QuoteStatus? Status { get; set; }
    public DateTime? ValidUntil { get; set; }
}

public class QuoteLineItemDto
{
    public int Id { get; set; }
    public int QuoteId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal TotalPrice { get; set; }
    public int Order { get; set; }
}

public class CreateQuoteLineItemDto
{
    public int ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public int Order { get; set; }
}

