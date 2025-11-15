using crm_backend.Modules.Communication;

namespace crm_backend.Modules.Communication.DTOs;

public class EmailDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? BodyHtml { get; set; }
    public string FromEmail { get; set; } = string.Empty;
    public string ToEmail { get; set; } = string.Empty;
    public string? CcEmail { get; set; }
    public string? BccEmail { get; set; }
    public EmailDirection Direction { get; set; }
    public EmailStatus Status { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public bool IsRead { get; set; }
    public string? ThreadId { get; set; }
    public int? ParentEmailId { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public int? OpportunityId { get; set; }
    public string? OpportunityName { get; set; }
    public int? TicketId { get; set; }
    public string? TicketTitle { get; set; }
    public int? SentByUserId { get; set; }
    public string? SentByUserName { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateEmailDto
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? BodyHtml { get; set; }
    public string FromEmail { get; set; } = string.Empty;
    public string ToEmail { get; set; } = string.Empty;
    public string? CcEmail { get; set; }
    public string? BccEmail { get; set; }
    public EmailDirection Direction { get; set; } = EmailDirection.Outbound;
    public string? ThreadId { get; set; }
    public int? ParentEmailId { get; set; }
    public int? CustomerId { get; set; }
    public int? ContactId { get; set; }
    public int? OpportunityId { get; set; }
    public int? TicketId { get; set; }
    public int? CompanyId { get; set; } // Optional, will be set from authenticated user
    public int? SentByUserId { get; set; } // Optional, will be set from authenticated user
}

public class UpdateEmailDto
{
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? BodyHtml { get; set; }
    public EmailStatus? Status { get; set; }
    public bool? IsRead { get; set; }
}

