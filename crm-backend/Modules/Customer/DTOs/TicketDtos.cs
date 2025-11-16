namespace crm_backend.Modules.Customer.DTOs;

public class TicketDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? Resolution { get; set; }
    public string? Tags { get; set; }
}

public class CreateTicketDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public int CustomerId { get; set; }
    public int? AssignedUserId { get; set; }
    public string? Tags { get; set; }
}

public class UpdateTicketDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TicketStatus? Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public int? AssignedUserId { get; set; }
    public string? Resolution { get; set; }
    public string? Tags { get; set; }
}

public enum TicketStatus
{
    Open,
    InProgress,
    WaitingForCustomer,
    Resolved,
    Closed
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Urgent
}
