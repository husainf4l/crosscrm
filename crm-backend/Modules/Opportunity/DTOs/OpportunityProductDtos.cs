namespace crm_backend.Modules.Opportunity.DTOs;

public class OpportunityProductDto
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreateOpportunityProductDto
{
    public int OpportunityId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
}

public class UpdateOpportunityProductDto
{
    public int? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
}

