namespace crm_backend.Modules.Financial;

// SyncStatus enum is defined in FinancialIntegration.cs

public class Payment
{
    public int Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty; // Auto-generated
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    
    // Payment Details
    public PaymentMethod Method { get; set; } = PaymentMethod.Cash;
    public string? TransactionId { get; set; } // External transaction ID
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    
    // Dates
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    
    // Relationships
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    
    public int CustomerId { get; set; }
    public Customer.Customer Customer { get; set; } = null!;
    
    public int? ReceivedByUserId { get; set; }
    public User.User? ReceivedByUser { get; set; }
    
    public int CreatedByUserId { get; set; }
    public User.User CreatedByUser { get; set; } = null!;
    
    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Financial Integration Fields
    // Note: Payments are synced from external financial systems
    // The CRM displays payment data but does not process payments directly
    public string? ExternalSystemId { get; set; } // ID in external system
    public string? ExternalSystemType { get; set; } // "QuickBooks", "Odoo", "SAP"
    public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
    public DateTime? LastSyncedAt { get; set; }
    public string? SyncError { get; set; } // Last sync error message
    public int? FinancialIntegrationId { get; set; } // Link to integration configuration
    public FinancialIntegration? FinancialIntegration { get; set; }
    
    // Navigation
    public ICollection<Customer.FileAttachment> Attachments { get; set; } = new List<Customer.FileAttachment>();
}

public enum PaymentMethod
{
    Cash,
    Check,
    CreditCard,
    BankTransfer,
    PayPal,
    Stripe,
    Other
}

