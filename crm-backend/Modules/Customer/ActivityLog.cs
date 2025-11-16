using System.ComponentModel.DataAnnotations;

namespace crm_backend.Modules.Customer;

public class ActivityLog
{
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    // Navigation property
    public Customer Customer { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string ActivityType { get; set; } = string.Empty; // "note", "ticket", "call", "email", "meeting", "file_upload", etc.

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    // Optional reference to related entity
    public string? ReferenceType { get; set; } // "ticket", "note", "file", etc.
    public int? ReferenceId { get; set; } // ID of the related entity

    // Who performed the activity
    [Required]
    public int UserId { get; set; }

    // Navigation property
    public crm_backend.Modules.User.User User { get; set; } = null!;

    [StringLength(1000)]
    public string? Details { get; set; } // Additional JSON data or notes

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Optional metadata
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
