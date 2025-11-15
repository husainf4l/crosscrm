namespace crm_backend.Modules.Collaboration;

public class AIAgentApiKeyUsageLog
{
    public int Id { get; set; }
    
    // Relationships
    public int ApiKeyId { get; set; }
    public AIAgentApiKey ApiKey { get; set; } = null!;
    
    // Request Details
    public string Endpoint { get; set; } = string.Empty; // e.g., "/api/ai-agent/execute-tool"
    public string Method { get; set; } = string.Empty; // GET, POST, PUT, DELETE
    public string? RequestPayload { get; set; } // JSON payload (optional, may be large)
    
    // Response Details
    public int ResponseStatus { get; set; } // HTTP status code
    public int ResponseTime { get; set; } // milliseconds
    
    // Request Metadata
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

