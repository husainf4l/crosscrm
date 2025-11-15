using HotChocolate;

namespace crm_backend.Modules.Collaboration.DTOs;

public class AIAgentApiKeyDto
{
    public int Id { get; set; }
    public int AgentId { get; set; }
    public string? AgentName { get; set; }
    public string KeyName { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty; // Only show prefix, never full key
    public bool IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<string> Permissions { get; set; } = new();
    public int? RateLimitPerMinute { get; set; }
    public int? RateLimitPerHour { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public int CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public int CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}

public class CreateAIAgentApiKeyDto
{
    public int AgentId { get; set; }
    public string KeyName { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public List<string>? Permissions { get; set; } // If null, grants all permissions (master key)
    public int? RateLimitPerMinute { get; set; }
    public int? RateLimitPerHour { get; set; }
}

public class CreateAIAgentApiKeyResponseDto
{
    public int Id { get; set; }
    public string ApiKey { get; set; } = string.Empty; // Only returned once on creation
    public string KeyPrefix { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UpdateAIAgentApiKeyDto
{
    public string? KeyName { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<string>? Permissions { get; set; }
    public int? RateLimitPerMinute { get; set; }
    public int? RateLimitPerHour { get; set; }
}

public class AIAgentToolDto
{
    public int Id { get; set; }
    public int AgentId { get; set; }
    public string? AgentName { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    
    [GraphQLIgnore]
    public Dictionary<string, object>? Parameters { get; set; } // JSON schema
    
    public List<string> Permissions { get; set; } = new();
    public bool IsActive { get; set; }
    public int CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public int CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateAIAgentToolDto
{
    public int AgentId { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty; // ToolType enum as string
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    
    [GraphQLIgnore]
    public Dictionary<string, object>? Parameters { get; set; } // JSON schema
    
    public List<string>? Permissions { get; set; }
}

public class UpdateAIAgentToolDto
{
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public string? Endpoint { get; set; }
    public string? Method { get; set; }
    
    [GraphQLIgnore]
    public Dictionary<string, object>? Parameters { get; set; }
    
    public List<string>? Permissions { get; set; }
    public bool? IsActive { get; set; }
}

public class ExecuteToolDto
{
    public string ToolName { get; set; } = string.Empty;
    
    [GraphQLIgnore]
    public Dictionary<string, object>? Parameters { get; set; }
}

public class ExecuteToolResponseDto
{
    public bool Success { get; set; }
    public object? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public int ExecutionTime { get; set; }
}

