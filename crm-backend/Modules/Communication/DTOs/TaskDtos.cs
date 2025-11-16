namespace crm_backend.Modules.Communication.DTOs;

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ReminderDate { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public int? OpportunityId { get; set; }
    public string? OpportunityName { get; set; }
    public int? TicketId { get; set; }
    public string? TicketTitle { get; set; }
    public int AssignedToUserId { get; set; }
    public string AssignedToUserName { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public DateTime? ReminderDate { get; set; }
    public int? CustomerId { get; set; }
    public int? ContactId { get; set; }
    public int? OpportunityId { get; set; }
    public int? TicketId { get; set; }
    public int AssignedToUserId { get; set; }
    public int? CompanyId { get; set; } // Optional, will be set from authenticated user
    public int? CreatedByUserId { get; set; } // Optional, will be set from authenticated user
}

public class UpdateTaskDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TaskStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReminderDate { get; set; }
    public int? AssignedToUserId { get; set; }
}

