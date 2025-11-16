namespace crm_backend.Modules.Customer.DTOs;

public class CustomerConnectionDto
{
    public List<CustomerEdgeDto> Edges { get; set; } = new();
    public PageInfoDto PageInfo { get; set; } = new();
    public int TotalCount { get; set; }
}

public class CustomerEdgeDto
{
    public CustomerDto Node { get; set; } = new();
    public string Cursor { get; set; } = string.Empty;
}

public class PageInfoDto
{
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public string? StartCursor { get; set; }
    public string? EndCursor { get; set; }
}

public class CustomerFiltersDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Status { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? CustomerType { get; set; }
    public string? Industry { get; set; }
    public string? Priority { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}
