using System.ComponentModel.DataAnnotations;

namespace crm_backend.Modules.Customer;

public class CustomerPreference
{
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    // Navigation property
    public Customer Customer { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string PreferenceKey { get; set; } = string.Empty; // "email_marketing", "sms_notifications", "call_preference", etc.

    [StringLength(500)]
    public string? PreferenceValue { get; set; } // "yes", "no", "morning", "evening", etc.

    [StringLength(100)]
    public string? Category { get; set; } // "communication", "marketing", "schedule", etc.

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Who set this preference
    public int? SetByUserId { get; set; }
    public crm_backend.Modules.User.User? SetByUser { get; set; }
}
