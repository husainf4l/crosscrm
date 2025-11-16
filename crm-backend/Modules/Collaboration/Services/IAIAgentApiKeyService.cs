using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.Services;

public interface IAIAgentApiKeyService
{
    Task<CreateAIAgentApiKeyResponseDto> GenerateApiKeyAsync(CreateAIAgentApiKeyDto dto, int companyId, int createdByUserId);
    Task<AIAgentApiKeyDto?> GetApiKeyByIdAsync(int id);
    Task<IEnumerable<AIAgentApiKeyDto>> GetApiKeysByAgentAsync(int agentId, int companyId);
    Task<AIAgentApiKeyDto?> UpdateApiKeyAsync(int id, UpdateAIAgentApiKeyDto dto, int companyId);
    Task<bool> RevokeApiKeyAsync(int id, int companyId);
    Task<AIAgentApiKey?> ValidateApiKeyAsync(string apiKey);
    Task<bool> CheckRateLimitAsync(int apiKeyId);
    Task LogApiKeyUsageAsync(int apiKeyId, string endpoint, string method, int responseStatus, int responseTime, string? ipAddress = null, string? userAgent = null, string? requestPayload = null);
}

