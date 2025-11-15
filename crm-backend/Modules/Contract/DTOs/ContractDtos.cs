using crm_backend.Modules.Contract;

namespace crm_backend.Modules.Contract.DTOs;

public class ContractDto
{
    public int Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    public bool AutoRenew { get; set; }
    public decimal TotalValue { get; set; }
    public string Currency { get; set; } = string.Empty;
    public ContractStatus Status { get; set; }
    public DateTime? SignedAt { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? OpportunityId { get; set; }
    public string? OpportunityName { get; set; }
    public int CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateContractDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    public bool AutoRenew { get; set; } = false;
    public decimal? TotalValue { get; set; }
    public string Currency { get; set; } = "USD";
    public int CustomerId { get; set; }
    public int? OpportunityId { get; set; }
    public int? CompanyId { get; set; } // Optional, will be set from authenticated user
    public int? CreatedByUserId { get; set; } // Optional, will be set from authenticated user
    public List<CreateContractLineItemDto> LineItems { get; set; } = new List<CreateContractLineItemDto>();
}

public class UpdateContractDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    public bool? AutoRenew { get; set; }
    public decimal? TotalValue { get; set; }
    public string? Currency { get; set; }
    public ContractStatus? Status { get; set; }
}

public class ContractLineItemDto
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public int Order { get; set; }
}

public class CreateContractLineItemDto
{
    public int ProductId { get; set; }
    public string? Description { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public int Order { get; set; }
}

