namespace crm_backend.Modules.Customer;

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    // Relationships
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int? AssignedUserId { get; set; }
    public crm_backend.Modules.User.User? AssignedUser { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Additional fields
    public string? Resolution { get; set; }
    public string? Tags { get; set; } // JSON array of tags
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
