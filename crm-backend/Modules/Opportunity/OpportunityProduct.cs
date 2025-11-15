namespace crm_backend.Modules.Opportunity;

public class OpportunityProduct
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public Opportunity Opportunity { get; set; } = null!;
    
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice * (1 - (DiscountPercent ?? 0) / 100);
}

