namespace crm_backend.Modules.Customer.DTOs;

public class CustomerNoteDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public NoteType Type { get; set; }
    public int CustomerId { get; set; }
    public int CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsPrivate { get; set; }
    public string? Tags { get; set; }
}

public class CreateCustomerNoteDto
{
    public string Content { get; set; } = string.Empty;
    public NoteType Type { get; set; } = NoteType.General;
    public int CustomerId { get; set; }
    public bool IsPrivate { get; set; } = false;
    public string? Tags { get; set; }
}

public class UpdateCustomerNoteDto
{
    public string? Content { get; set; }
    public NoteType? Type { get; set; }
    public bool? IsPrivate { get; set; }
    public string? Tags { get; set; }
}

public enum NoteType
{
    General,
    Call,
    Meeting,
    Email,
    FollowUp,
    Complaint,
    Internal
}
