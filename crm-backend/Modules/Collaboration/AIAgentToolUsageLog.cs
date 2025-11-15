namespace crm_backend.Modules.Collaboration;

public class AIAgentToolUsageLog
{
    public int Id { get; set; }
    
    // Relationships
    public int ToolId { get; set; }
    public AIAgentTool Tool { get; set; } = null!;
    
    public int ApiKeyId { get; set; }
    public AIAgentApiKey ApiKey { get; set; } = null!;
    
    // Execution Details
    public string Parameters { get; set; } = "{}"; // JSON parameters passed to tool
    public string? Result { get; set; } // JSON result from tool execution
    public ToolExecutionStatus Status { get; set; } = ToolExecutionStatus.Success;
    public string? ErrorMessage { get; set; }
    public int ExecutionTime { get; set; } // milliseconds
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum ToolExecutionStatus
{
    Success,
    Failed,
    Error
}

