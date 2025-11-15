using System.Net.Http.Json;
using System.Text.Json;
using crm_backend.Data;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Collaboration.Services;

public class AIAgentClientService : IAIAgentClientService
{
    private readonly HttpClient _httpClient;
    private readonly CrmDbContext _context;
    private readonly ILogger<AIAgentClientService> _logger;

    public AIAgentClientService(HttpClient httpClient, CrmDbContext context, ILogger<AIAgentClientService> logger)
    {
        _httpClient = httpClient;
        _context = context;
        _logger = logger;
    }

    public async Task<string> SendRequestAsync(int agentId, string input, string? entityType = null, int? entityId = null, int? userId = null)
    {
        // Get agent configuration
        var agent = await _context.AIAgents.FindAsync(agentId);
        if (agent == null)
        {
            throw new InvalidOperationException($"Agent with ID {agentId} not found.");
        }

        if (agent.Status != AgentStatus.Active)
        {
            throw new InvalidOperationException($"Agent {agent.Name} is not active.");
        }

        var pythonServiceUrl = agent.PythonServiceUrl.TrimEnd('/');
        var requestUrl = $"{pythonServiceUrl}/api/agents/{agent.AgentId}/execute";

        try
        {
            // Prepare request payload
            var requestPayload = new
            {
                input = input,
                entity_type = entityType,
                entity_id = entityId,
                user_id = userId,
                agent_config = new
                {
                    system_prompt = agent.SystemPrompt,
                    tools = JsonSerializer.Deserialize<List<string>>(agent.Tools) ?? new List<string>()
                }
            };

            _logger?.LogInformation($"Sending request to Python service: {requestUrl}");

            var response = await _httpClient.PostAsJsonAsync(requestUrl, requestPayload);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger?.LogError($"Python service returned error: {response.StatusCode} - {errorContent}");
                throw new HttpRequestException($"Python service returned error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);

            // Extract response text or result
            if (responseJson.TryGetProperty("result", out var result))
            {
                return result.GetString() ?? responseContent;
            }
            else if (responseJson.TryGetProperty("output", out var output))
            {
                return output.GetString() ?? responseContent;
            }
            else if (responseJson.TryGetProperty("response", out var responseProp))
            {
                return responseProp.GetString() ?? responseContent;
            }

            return responseContent;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, $"Error communicating with Python service: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Unexpected error sending request to Python service: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> HealthCheckAsync(string? pythonServiceUrl = null)
    {
        var url = pythonServiceUrl ?? "http://localhost:8003";
        url = url.TrimEnd('/');
        var healthCheckUrl = $"{url}/health";

        try
        {
            _logger?.LogInformation($"Checking Python service health: {healthCheckUrl}");
            var response = await _httpClient.GetAsync(healthCheckUrl);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Health check failed for Python service: {ex.Message}");
            return false;
        }
    }
}

