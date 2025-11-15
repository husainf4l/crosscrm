namespace crm_backend.Modules.Communication;

public class Appointment
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; } // Physical location or video link
    
    // Scheduling
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? TimeZone { get; set; }
    public bool IsAllDay { get; set; } = false;
    
    // Type
    public AppointmentType Type { get; set; } = AppointmentType.Meeting;
    
    // Additional type for CRM categorization
    public AppointmentCategory Category { get; set; } = AppointmentCategory.Sales;
    
    // Status
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    
    // Reminders
    public int? ReminderMinutesBefore { get; set; } // 15, 30, 60, etc.
    public bool ReminderSent { get; set; } = false;
    
    // Relationships
    public int? CustomerId { get; set; }
    public Customer.Customer? Customer { get; set; }
    
    public int? ContactId { get; set; }
    public Customer.Contact? Contact { get; set; }
    
    public int? OpportunityId { get; set; }
    public Opportunity.Opportunity? Opportunity { get; set; }
    
    public int? QuoteId { get; set; }
    public Financial.Quote? Quote { get; set; }
    
    public int CreatedByUserId { get; set; }
    public User.User CreatedByUser { get; set; } = null!;
    
    // Attendees (many-to-many)
    public ICollection<AppointmentAttendee> Attendees { get; set; } = new List<AppointmentAttendee>();
    
    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<Customer.FileAttachment> Attachments { get; set; } = new List<Customer.FileAttachment>();
}

public enum AppointmentType
{
    Meeting,
    Call,
    Demo,
    Presentation,
    FollowUp,
    Other
}

public enum AppointmentStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
    NoShow
}

public enum AppointmentCategory
{
    Sales,
    Support,
    Demo,
    Meeting,
    Other
}

