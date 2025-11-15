using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.Services;

public interface IAIAgentToolService
{
    Task<IEnumerable<AIAgentToolDto>> GetToolsByAgentAsync(int agentId, int companyId);
    Task<AIAgentToolDto?> GetToolByIdAsync(int id);
    Task<AIAgentToolDto?> GetToolByNameAsync(int agentId, string toolName, int companyId);
    Task<AIAgentToolDto> CreateToolAsync(CreateAIAgentToolDto dto, int companyId, int createdByUserId);
    Task<AIAgentToolDto?> UpdateToolAsync(int id, UpdateAIAgentToolDto dto, int companyId);
    Task<bool> DeleteToolAsync(int id, int companyId);
    Task<ExecuteToolResponseDto> ExecuteToolAsync(string toolName, Dictionary<string, object>? parameters, int apiKeyId, int companyId);
    Task LogToolUsageAsync(int toolId, int apiKeyId, Dictionary<string, object>? parameters, object? result, ToolExecutionStatus status, int executionTime, string? errorMessage = null);
}

