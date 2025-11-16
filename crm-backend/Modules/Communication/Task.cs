namespace crm_backend.Modules.Communication;

public class Task
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Type
    public TaskType Type { get; set; } = TaskType.Other;

    // Status
    public TaskStatus Status { get; set; } = TaskStatus.NotStarted;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    // Dates
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ReminderDate { get; set; }

    // Relationships
    public int? CustomerId { get; set; }
    public Customer.Customer? Customer { get; set; }

    public int? ContactId { get; set; }
    public Customer.Contact? Contact { get; set; }

    public int? OpportunityId { get; set; }
    public Opportunity.Opportunity? Opportunity { get; set; }

    public int? QuoteId { get; set; }
    public Financial.Quote? Quote { get; set; }

    public int? InvoiceId { get; set; }
    public Financial.Invoice? Invoice { get; set; }

    public int? ContractId { get; set; }
    public Contract.Contract? Contract { get; set; }

    public int? TicketId { get; set; }
    public Customer.Ticket? Ticket { get; set; }

    public int AssignedToUserId { get; set; }
    public User.User AssignedToUser { get; set; } = null!;

    public int CreatedByUserId { get; set; }
    public User.User CreatedByUser { get; set; } = null!;

    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum TaskStatus
{
    NotStarted,
    InProgress,
    Completed,
    Cancelled,
    OnHold
}

public enum TaskPriority
{
    Low,
    Medium,
    High,
    Urgent
}

public enum TaskType
{
    FollowUp,
    Call,
    Meeting,
    Email,
    Other
}

