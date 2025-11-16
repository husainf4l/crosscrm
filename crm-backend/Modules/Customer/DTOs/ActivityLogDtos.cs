using System.ComponentModel.DataAnnotations;

namespace crm_backend.Modules.Customer.DTOs;

public class ActivityLogDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? IpAddress { get; set; }
}

public class CreateActivityLogDto
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    [StringLength(100)]
    public string ActivityType { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }

    [StringLength(1000)]
    public string? Details { get; set; }
}

public class ActivityLogSummaryDto
{
    public string ActivityType { get; set; } = string.Empty;
    public int Count { get; set; }
    public DateTime LastActivity { get; set; }
}
