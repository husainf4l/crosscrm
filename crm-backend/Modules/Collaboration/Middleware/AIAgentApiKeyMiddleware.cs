using crm_backend.Modules.Collaboration.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace crm_backend.Modules.Collaboration.Middleware;

public class AIAgentApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AIAgentApiKeyMiddleware> _logger;

    public AIAgentApiKeyMiddleware(RequestDelegate next, ILogger<AIAgentApiKeyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAIAgentApiKeyService apiKeyService)
    {
        // Only apply to API routes
        if (!context.Request.Path.StartsWithSegments("/api/ai-agent"))
        {
            await _next(context);
            return;
        }

        // Extract API key from header
        string? apiKey = null;
        
        // Try Authorization header (Bearer token)
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var authValue = authHeader.ToString();
            if (authValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                apiKey = authValue.Substring("Bearer ".Length).Trim();
            }
            else if (authValue.StartsWith("ApiKey ", StringComparison.OrdinalIgnoreCase))
            {
                apiKey = authValue.Substring("ApiKey ".Length).Trim();
            }
        }

        // Try X-API-Key header
        if (string.IsNullOrEmpty(apiKey) && context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
        {
            apiKey = apiKeyHeader.ToString();
        }

        if (string.IsNullOrEmpty(apiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "API key is required" });
            return;
        }

        // Validate API key
        var apiKeyEntity = await apiKeyService.ValidateApiKeyAsync(apiKey);
        if (apiKeyEntity == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API key" });
            return;
        }

        // Check rate limit
        var rateLimitOk = await apiKeyService.CheckRateLimitAsync(apiKeyEntity.Id);
        if (!rateLimitOk)
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsJsonAsync(new { error = "Rate limit exceeded" });
            return;
        }

        // Add agent context to request
        context.Items["AIAgentId"] = apiKeyEntity.AgentId;
        context.Items["AIAgentApiKeyId"] = apiKeyEntity.Id;
        context.Items["AIAgentCompanyId"] = apiKeyEntity.CompanyId;
        context.Items["AIAgentPermissions"] = apiKeyEntity.Permissions;

        // Add claims for authorization
        var claims = new List<Claim>
        {
            new Claim("AIAgentId", apiKeyEntity.AgentId.ToString()),
            new Claim("AIAgentApiKeyId", apiKeyEntity.Id.ToString()),
            new Claim("CompanyId", apiKeyEntity.CompanyId.ToString()),
            new Claim("AuthType", "ApiKey")
        };

        var identity = new ClaimsIdentity(claims, "ApiKey");
        context.User = new ClaimsPrincipal(identity);

        await _next(context);
    }
}

