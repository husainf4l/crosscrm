namespace crm_backend.Modules.Collaboration.DTOs;

public class AIAgentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public List<string> Tools { get; set; } = new();
    public string PythonServiceUrl { get; set; } = string.Empty;
    public string AgentId { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public int CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateAIAgentDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty; // AgentType enum as string
    public string SystemPrompt { get; set; } = string.Empty;
    public List<string> Tools { get; set; } = new();
    public string? PythonServiceUrl { get; set; }
}

public class UpdateAIAgentDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? SystemPrompt { get; set; }
    public List<string>? Tools { get; set; }
    public string? PythonServiceUrl { get; set; }
}

public class AIAgentAssignmentDto
{
    public int Id { get; set; }
    public int AgentId { get; set; }
    public string? AgentName { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int AssignedByUserId { get; set; }
    public string? AssignedByUserName { get; set; }
    public DateTime AssignedAt { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAIAgentAssignmentDto
{
    public int AgentId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAIAgentAssignmentDto
{
    public bool? IsActive { get; set; }
    public string? Notes { get; set; }
}

public class AIAgentInteractionDto
{
    public int Id { get; set; }
    public int AgentId { get; set; }
    public string? AgentName { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string Input { get; set; } = string.Empty;
    public string? Output { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RequestId { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class CreateAIAgentInteractionDto
{
    public int AgentId { get; set; }
    public string Type { get; set; } = string.Empty; // InteractionType enum as string
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string Input { get; set; } = string.Empty;
}

public class UpdateAIAgentInteractionDto
{
    public string? Output { get; set; }
    public string? Status { get; set; }
    public string? RequestId { get; set; }
}

