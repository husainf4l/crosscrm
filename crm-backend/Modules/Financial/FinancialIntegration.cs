namespace crm_backend.Modules.Financial;

/// <summary>
/// Represents a connection to an external financial system (QuickBooks, Odoo, SAP)
/// The CRM syncs financial data from these systems rather than maintaining a full financial model
/// </summary>
public class FinancialIntegration
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // e.g., "QuickBooks Production"
    public string SystemType { get; set; } = string.Empty; // "QuickBooks", "Odoo", "SAP"
    public string? Description { get; set; }
    
    // Connection Settings (encrypted JSON)
    public string ConnectionSettings { get; set; } = string.Empty; // Encrypted JSON with OAuth tokens, API keys, etc.
    
    // Sync Configuration
    public bool IsActive { get; set; } = true;
    public string SyncFrequency { get; set; } = "Hourly"; // "RealTime", "Hourly", "Daily", "Weekly", "Manual"
    public List<string> SyncEntities { get; set; } = new List<string>(); // ["Customer", "Invoice", "Payment", "Order", "Balance"]
    
    // Status
    public IntegrationStatus Status { get; set; } = IntegrationStatus.Disconnected;
    public DateTime? LastSyncAt { get; set; }
    public string? LastSyncError { get; set; }
    public int? LastSyncCount { get; set; }
    
    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public DateTime? DisconnectedAt { get; set; }
    
    // Navigation
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<SyncHistory> SyncHistory { get; set; } = new List<SyncHistory>();
}

public enum IntegrationStatus
{
    Disconnected,
    Connecting,
    Connected,
    Error,
    Disabled
}

public enum SyncStatus
{
    Pending,
    Syncing,
    Synced,
    SyncFailed,
    Conflict
}

/// <summary>
/// Tracks sync history for financial integrations
/// </summary>
public class SyncHistory
{
    public int Id { get; set; }
    public int FinancialIntegrationId { get; set; }
    public FinancialIntegration FinancialIntegration { get; set; } = null!;
    
    public string EntityType { get; set; } = string.Empty; // "Customer", "Invoice", "Payment", "Order", "Balance"
    public int? EntityId { get; set; } // ID of synced entity in CRM
    public string? ExternalEntityId { get; set; } // ID in external system
    
    public SyncStatus SyncStatus { get; set; }
    public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }
    public string? SyncDetails { get; set; } // JSON with sync details
    
    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
}

