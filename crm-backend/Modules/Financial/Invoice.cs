namespace crm_backend.Modules.Financial;

// SyncStatus enum is defined in FinancialIntegration.cs

public class Invoice
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty; // Auto-generated: "INV-2025-001"
    public string? Title { get; set; }
    public string? Description { get; set; }

    // Financial
    public decimal SubTotal { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; } = 0;
    public decimal BalanceAmount => TotalAmount - PaidAmount;
    public string Currency { get; set; } = "USD";

    // Dates
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }

    // Status
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    // Relationships
    public int CustomerId { get; set; }
    public Customer.Customer Customer { get; set; } = null!;

    public int? QuoteId { get; set; } // Converted from quote
    public Quote? Quote { get; set; }

    public int? OpportunityId { get; set; }
    public Opportunity.Opportunity? Opportunity { get; set; }

    public int? AssignedToTeamId { get; set; }
    public Collaboration.Team? AssignedTeam { get; set; }

    public int CreatedByUserId { get; set; }
    public User.User CreatedByUser { get; set; } = null!;

    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Financial Integration Fields
    // Note: Invoices are primarily synced from external financial systems (QuickBooks, Odoo, SAP)
    // The CRM displays invoice data but does not manage the full financial lifecycle
    public string? ExternalSystemId { get; set; } // ID in external system (QuickBooks, Odoo, SAP)
    public string? ExternalSystemType { get; set; } // "QuickBooks", "Odoo", "SAP"
    public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
    public DateTime? LastSyncedAt { get; set; }
    public string? SyncError { get; set; } // Last sync error message
    public int? FinancialIntegrationId { get; set; } // Link to integration configuration
    public FinancialIntegration? FinancialIntegration { get; set; }

    // Navigation
    public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Customer.FileAttachment> Attachments { get; set; } = new List<Customer.FileAttachment>();
    public ICollection<Contract.Contract> Contracts { get; set; } = new List<Contract.Contract>();
}

public enum InvoiceStatus
{
    Draft,
    Sent,
    Paid,
    PartiallyPaid,
    Overdue,
    Cancelled,
    Refunded
}

