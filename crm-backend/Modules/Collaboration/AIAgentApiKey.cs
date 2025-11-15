using System.Text.Json;

namespace crm_backend.Modules.Collaboration;

public class AIAgentApiKey
{
    public int Id { get; set; }
    
    // Relationships
    public int AgentId { get; set; }
    public AIAgent Agent { get; set; } = null!;
    
    // Key Information
    public string KeyName { get; set; } = string.Empty; // Descriptive name
    public string ApiKeyHash { get; set; } = string.Empty; // Hashed API key (never store plain text)
    public string KeyPrefix { get; set; } = string.Empty; // First 8-12 chars for display (e.g., "sk_live_abc")
    
    // Configuration
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
    public string Permissions { get; set; } = "[]"; // JSON array of permission scopes
    public int? RateLimitPerMinute { get; set; } // Optional rate limiting
    public int? RateLimitPerHour { get; set; }
    
    // Tracking
    public DateTime? LastUsedAt { get; set; }
    
    // Relationships
    public int CreatedByUserId { get; set; }
    public User.User CreatedByUser { get; set; } = null!;
    
    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    
    // Navigation
    public ICollection<AIAgentApiKeyUsageLog> UsageLogs { get; set; } = new List<AIAgentApiKeyUsageLog>();
}

