using crm_backend.Modules.Communication;

namespace crm_backend.Modules.Communication.DTOs;

public class AppointmentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? TimeZone { get; set; }
    public bool IsAllDay { get; set; }
    public AppointmentType Type { get; set; }
    public AppointmentStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public int? ReminderMinutesBefore { get; set; }
    public bool ReminderSent { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public int? OpportunityId { get; set; }
    public string? OpportunityName { get; set; }
    public int CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateAppointmentDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? TimeZone { get; set; }
    public bool IsAllDay { get; set; } = false;
    public AppointmentType Type { get; set; } = AppointmentType.Meeting;
    public int? ReminderMinutesBefore { get; set; }
    public int? CustomerId { get; set; }
    public int? ContactId { get; set; }
    public int? OpportunityId { get; set; }
    public int? CompanyId { get; set; } // Optional, will be set from authenticated user
    public int? CreatedByUserId { get; set; } // Optional, will be set from authenticated user
    public List<CreateAppointmentAttendeeDto> Attendees { get; set; } = new List<CreateAppointmentAttendeeDto>();
}

public class UpdateAppointmentDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? TimeZone { get; set; }
    public bool? IsAllDay { get; set; }
    public AppointmentType? Type { get; set; }
    public AppointmentStatus? Status { get; set; }
    public string? CancellationReason { get; set; }
    public int? ReminderMinutesBefore { get; set; }
}

public class AppointmentAttendeeDto
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public AttendeeStatus Status { get; set; }
    public DateTime? RespondedAt { get; set; }
}

public class CreateAppointmentAttendeeDto
{
    public int? UserId { get; set; }
    public int? ContactId { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
}

