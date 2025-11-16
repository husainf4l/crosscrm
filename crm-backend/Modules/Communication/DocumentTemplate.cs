namespace crm_backend.Modules.Communication;

/// <summary>
/// Document Template for generating documents (Quote, Invoice, Contract)
/// </summary>
public class DocumentTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DocumentTemplateType Type { get; set; } // Quote, Invoice, Contract, etc.

    // Template Content
    public string TemplateContent { get; set; } = string.Empty; // HTML or PDF template
    public string? Variables { get; set; } // JSON: Available placeholders/variables

    // Template Settings
    public bool IsDefault { get; set; } = false; // Default template for this type
    public bool IsActive { get; set; } = true;

    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int CreatedByUserId { get; set; }
    public User.User CreatedByUser { get; set; } = null!;
}

public enum DocumentTemplateType
{
    Quote,
    Invoice,
    Contract,
    Proposal,
    Statement,
    Other
}

/// <summary>
/// Email Template for sending standardized emails
/// </summary>
public class EmailTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EmailTemplateType Type { get; set; } // Welcome, Quote, Invoice, etc.

    // Email Content
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty; // HTML or plain text
    public string? Variables { get; set; } // JSON: Available placeholders/variables

    // Template Settings
    public bool IsDefault { get; set; } = false; // Default template for this type
    public bool IsActive { get; set; } = true;

    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int CreatedByUserId { get; set; }
    public User.User CreatedByUser { get; set; } = null!;
}

public enum EmailTemplateType
{
    Welcome,
    Quote,
    Invoice,
    PaymentReminder,
    Contract,
    FollowUp,
    Other
}

