namespace crm_backend.Modules.Collaboration;

public class AIAgentTool
{
    public int Id { get; set; }

    // Relationships
    public int AgentId { get; set; }
    public AIAgent Agent { get; set; } = null!;

    // Tool Information
    public string ToolName { get; set; } = string.Empty; // Unique identifier (e.g., "read_customer", "create_note")
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Tool Configuration
    public ToolType Type { get; set; } = ToolType.Read;
    public string Endpoint { get; set; } = string.Empty; // API endpoint (e.g., "/api/customers/{id}")
    public string Method { get; set; } = "GET"; // HTTP method

    // Parameters Schema (JSON Schema format)
    public string Parameters { get; set; } = "{}"; // JSON schema defining tool parameters

    // Permissions (JSON array of required permissions)
    public string Permissions { get; set; } = "[]";

    // Status
    public bool IsActive { get; set; } = true;

    // Relationships
    public int CreatedByUserId { get; set; }
    public User.User CreatedByUser { get; set; } = null!;

    // Multi-tenant
    public int CompanyId { get; set; }
    public Company.Company Company { get; set; } = null!;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<AIAgentToolUsageLog> UsageLogs { get; set; } = new List<AIAgentToolUsageLog>();
}

public enum ToolType
{
    Read,
    Write,
    Delete,
    Query,
    Action
}

