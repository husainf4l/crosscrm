namespace crm_backend.Modules.Financial;

public class InvoiceLineItem
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    public int ProductId { get; set; }
    public Opportunity.Product Product { get; set; } = null!;

    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice * (1 - (DiscountPercent ?? 0) / 100);

    public int Order { get; set; }
}

