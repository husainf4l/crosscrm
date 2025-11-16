namespace crm_backend.Modules.Communication;

public class AppointmentAttendee
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;

    public int? UserId { get; set; } // Internal user
    public User.User? User { get; set; }

    public int? ContactId { get; set; } // External contact
    public Customer.Contact? Contact { get; set; }

    public string? Email { get; set; } // External email if not a contact
    public string? Name { get; set; }

    public AttendeeStatus Status { get; set; } = AttendeeStatus.Invited;
    public DateTime? RespondedAt { get; set; }
}

public enum AttendeeStatus
{
    Invited,
    Accepted,
    Declined,
    Tentative,
    NoResponse
}

