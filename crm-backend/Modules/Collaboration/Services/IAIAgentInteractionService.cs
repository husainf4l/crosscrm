using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.Services;

public interface IAIAgentInteractionService
{
    Task<AIAgentInteractionDto> CreateInteractionAsync(CreateAIAgentInteractionDto dto, int companyId, int? userId = null);
    Task<AIAgentInteractionDto?> UpdateInteractionAsync(int id, UpdateAIAgentInteractionDto dto);
    Task<AIAgentInteractionDto?> GetInteractionByIdAsync(int id);
    Task<IEnumerable<AIAgentInteractionDto>> GetInteractionsByAgentAsync(int agentId, int? companyId = null);
    Task<IEnumerable<AIAgentInteractionDto>> GetInteractionsByEntityAsync(string entityType, int entityId, int companyId);
    Task<IEnumerable<AIAgentInteractionDto>> GetInteractionsByUserAsync(int userId, int companyId);
}

