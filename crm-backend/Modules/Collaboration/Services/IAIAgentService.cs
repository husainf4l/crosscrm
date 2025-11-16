using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.Services;

public interface IAIAgentService
{
    Task<IEnumerable<AIAgentDto>> GetAllAgentsAsync(int? companyId = null);
    Task<AIAgentDto?> GetAgentByIdAsync(int id);
    Task<AIAgentDto> CreateAgentAsync(CreateAIAgentDto dto, int companyId, int createdByUserId);
    Task<AIAgentDto?> UpdateAgentAsync(int id, UpdateAIAgentDto dto);
    Task<bool> DeleteAgentAsync(int id);
    Task<AIAgentAssignmentDto> AssignAgentAsync(CreateAIAgentAssignmentDto dto, int companyId, int assignedByUserId);
    Task<bool> UnassignAgentAsync(int assignmentId);
    Task<IEnumerable<AIAgentAssignmentDto>> GetAgentAssignmentsAsync(int agentId);
    Task<IEnumerable<AIAgentAssignmentDto>> GetAssignmentsByEntityAsync(string entityType, int entityId, int companyId);
}

