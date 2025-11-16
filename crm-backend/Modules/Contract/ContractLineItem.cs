namespace crm_backend.Modules.Contract;

public class ContractLineItem
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public Contract Contract { get; set; } = null!;

    public int ProductId { get; set; }
    public Opportunity.Product Product { get; set; } = null!;

    public string? Description { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; } // Calculated: Quantity * UnitPrice
    public int Order { get; set; } // Display order
}

