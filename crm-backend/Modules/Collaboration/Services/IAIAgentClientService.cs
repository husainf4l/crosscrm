namespace crm_backend.Modules.Collaboration.Services;

public interface IAIAgentClientService
{
    Task<string> SendRequestAsync(int agentId, string input, string? entityType = null, int? entityId = null, int? userId = null);
    Task<bool> HealthCheckAsync(string? pythonServiceUrl = null);
}

