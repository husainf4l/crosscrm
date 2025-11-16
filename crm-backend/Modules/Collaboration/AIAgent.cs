namespace crm_backend.Modules.Collaboration;

public class AIAgent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Agent Configuration
    public AgentType Type { get; set; } = AgentType.CustomerService;
    public AgentStatus Status { get; set; } = AgentStatus.Active;

    // LangGraph Configuration (Python service)
    public string SystemPrompt { get; set; } = string.Empty; // The prompt template for the agent
    public string Tools { get; set; } = "[]"; // JSON array of available tools: ["read_customer", "create_note", "analyze_data", "send_message"]
    public string PythonServiceUrl { get; set; } = "http://localhost:8003"; // Default Python service URL
    public string AgentId { get; set; } = string.Empty; // Unique identifier for Python service (UUID)

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
    public ICollection<AIAgentAssignment> Assignments { get; set; } = new List<AIAgentAssignment>();
    public ICollection<AIAgentInteraction> Interactions { get; set; } = new List<AIAgentInteraction>();
}

public enum AgentType
{
    CustomerService,
    SalesAssistant,
    DataAnalyst,
    StrategyAdvisor
}

public enum AgentStatus
{
    Active,
    Inactive,
    Training
}

