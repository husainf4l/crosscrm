using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BCrypt.Net;
using crm_backend.Data;
using crm_backend.Modules.Collaboration;
using crm_backend.Modules.Collaboration.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Collaboration.Services;

public class AIAgentApiKeyService : IAIAgentApiKeyService
{
    private readonly CrmDbContext _context;

    public AIAgentApiKeyService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<CreateAIAgentApiKeyResponseDto> GenerateApiKeyAsync(CreateAIAgentApiKeyDto dto, int companyId, int createdByUserId)
    {
        // Verify agent exists and belongs to company
        var agent = await _context.AIAgents
            .FirstOrDefaultAsync(a => a.Id == dto.AgentId && a.CompanyId == companyId);
        if (agent == null)
        {
            throw new InvalidOperationException("Agent not found or does not belong to the company.");
        }

        // Generate secure API key
        var keySuffix = GenerateSecureApiKey();
        var fullApiKey = $"sk_live_{keySuffix}";
        var keyPrefix = $"sk_live_{keySuffix.Substring(0, 8)}";
        var apiKeyHash = BCrypt.Net.BCrypt.HashPassword(fullApiKey); // Hash the full key including prefix

        var apiKeyEntity = new AIAgentApiKey
        {
            AgentId = dto.AgentId,
            KeyName = dto.KeyName,
            ApiKeyHash = apiKeyHash,
            KeyPrefix = keyPrefix,
            IsActive = true,
            ExpiresAt = dto.ExpiresAt,
            Permissions = dto.Permissions != null ? JsonSerializer.Serialize(dto.Permissions) : "[]", // Empty array = all permissions (master key)
            RateLimitPerMinute = dto.RateLimitPerMinute,
            RateLimitPerHour = dto.RateLimitPerHour,
            CreatedByUserId = createdByUserId,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.AIAgentApiKeys.Add(apiKeyEntity);
        await _context.SaveChangesAsync();

        // Return response with the plain key (only shown once)
        return new CreateAIAgentApiKeyResponseDto
        {
            Id = apiKeyEntity.Id,
            ApiKey = fullApiKey, // Full key with prefix
            KeyPrefix = keyPrefix,
            CreatedAt = apiKeyEntity.CreatedAt
        };
    }

    public async Task<AIAgentApiKeyDto?> GetApiKeyByIdAsync(int id)
    {
        var apiKey = await _context.AIAgentApiKeys
            .Include(ak => ak.Agent)
            .Include(ak => ak.CreatedByUser)
            .FirstOrDefaultAsync(ak => ak.Id == id);

        if (apiKey == null) return null;

        return new AIAgentApiKeyDto
        {
            Id = apiKey.Id,
            AgentId = apiKey.AgentId,
            AgentName = apiKey.Agent.Name,
            KeyName = apiKey.KeyName,
            KeyPrefix = apiKey.KeyPrefix,
            IsActive = apiKey.IsActive,
            ExpiresAt = apiKey.ExpiresAt,
            Permissions = !string.IsNullOrEmpty(apiKey.Permissions)
                ? JsonSerializer.Deserialize<List<string>>(apiKey.Permissions) ?? new List<string>()
                : new List<string>(),
            RateLimitPerMinute = apiKey.RateLimitPerMinute,
            RateLimitPerHour = apiKey.RateLimitPerHour,
            LastUsedAt = apiKey.LastUsedAt,
            CreatedByUserId = apiKey.CreatedByUserId,
            CreatedByUserName = apiKey.CreatedByUser.Name,
            CompanyId = apiKey.CompanyId,
            CreatedAt = apiKey.CreatedAt,
            UpdatedAt = apiKey.UpdatedAt,
            RevokedAt = apiKey.RevokedAt
        };
    }

    public async Task<IEnumerable<AIAgentApiKeyDto>> GetApiKeysByAgentAsync(int agentId, int companyId)
    {
        var apiKeys = await _context.AIAgentApiKeys
            .Include(ak => ak.Agent)
            .Include(ak => ak.CreatedByUser)
            .Where(ak => ak.AgentId == agentId && ak.CompanyId == companyId)
            .Select(ak => new AIAgentApiKeyDto
            {
                Id = ak.Id,
                AgentId = ak.AgentId,
                AgentName = ak.Agent.Name,
                KeyName = ak.KeyName,
                KeyPrefix = ak.KeyPrefix,
                IsActive = ak.IsActive,
                ExpiresAt = ak.ExpiresAt,
                Permissions = !string.IsNullOrEmpty(ak.Permissions)
                    ? JsonSerializer.Deserialize<List<string>>(ak.Permissions) ?? new List<string>()
                    : new List<string>(),
                RateLimitPerMinute = ak.RateLimitPerMinute,
                RateLimitPerHour = ak.RateLimitPerHour,
                LastUsedAt = ak.LastUsedAt,
                CreatedByUserId = ak.CreatedByUserId,
                CreatedByUserName = ak.CreatedByUser.Name,
                CompanyId = ak.CompanyId,
                CreatedAt = ak.CreatedAt,
                UpdatedAt = ak.UpdatedAt,
                RevokedAt = ak.RevokedAt
            })
            .ToListAsync();

        return apiKeys;
    }

    public async Task<AIAgentApiKeyDto?> UpdateApiKeyAsync(int id, UpdateAIAgentApiKeyDto dto, int companyId)
    {
        var apiKey = await _context.AIAgentApiKeys
            .FirstOrDefaultAsync(ak => ak.Id == id && ak.CompanyId == companyId);
        if (apiKey == null) return null;

        if (!string.IsNullOrEmpty(dto.KeyName))
            apiKey.KeyName = dto.KeyName;

        if (dto.IsActive.HasValue)
            apiKey.IsActive = dto.IsActive.Value;

        if (dto.ExpiresAt.HasValue)
            apiKey.ExpiresAt = dto.ExpiresAt;

        if (dto.Permissions != null)
            apiKey.Permissions = JsonSerializer.Serialize(dto.Permissions);

        if (dto.RateLimitPerMinute.HasValue)
            apiKey.RateLimitPerMinute = dto.RateLimitPerMinute;

        if (dto.RateLimitPerHour.HasValue)
            apiKey.RateLimitPerHour = dto.RateLimitPerHour;

        apiKey.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetApiKeyByIdAsync(apiKey.Id);
    }

    public async Task<bool> RevokeApiKeyAsync(int id, int companyId)
    {
        var apiKey = await _context.AIAgentApiKeys
            .FirstOrDefaultAsync(ak => ak.Id == id && ak.CompanyId == companyId);
        if (apiKey == null) return false;

        apiKey.IsActive = false;
        apiKey.RevokedAt = DateTime.UtcNow;
        apiKey.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<AIAgentApiKey?> ValidateApiKeyAsync(string apiKey)
    {
        // Get all active API keys
        var apiKeys = await _context.AIAgentApiKeys
            .Include(ak => ak.Agent)
            .Where(ak => ak.IsActive && (ak.ExpiresAt == null || ak.ExpiresAt > DateTime.UtcNow))
            .ToListAsync();

        // Try to match the key by verifying hash
        foreach (var key in apiKeys)
        {
            // Check if the provided key starts with the stored prefix (quick check)
            if (!apiKey.StartsWith(key.KeyPrefix))
                continue;

            // Verify the full key hash
            if (BCrypt.Net.BCrypt.Verify(apiKey, key.ApiKeyHash))
            {
                // Update last used
                key.LastUsedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                return key;
            }
        }

        return null;
    }

    public async Task<bool> CheckRateLimitAsync(int apiKeyId)
    {
        var apiKey = await _context.AIAgentApiKeys.FindAsync(apiKeyId);
        if (apiKey == null || !apiKey.IsActive) return false;

        var now = DateTime.UtcNow;

        // Check per-minute limit
        if (apiKey.RateLimitPerMinute.HasValue)
        {
            var oneMinuteAgo = now.AddMinutes(-1);
            var recentRequests = await _context.AIAgentApiKeyUsageLogs
                .CountAsync(log => log.ApiKeyId == apiKeyId && log.CreatedAt >= oneMinuteAgo);
            
            if (recentRequests >= apiKey.RateLimitPerMinute.Value)
                return false;
        }

        // Check per-hour limit
        if (apiKey.RateLimitPerHour.HasValue)
        {
            var oneHourAgo = now.AddHours(-1);
            var recentRequests = await _context.AIAgentApiKeyUsageLogs
                .CountAsync(log => log.ApiKeyId == apiKeyId && log.CreatedAt >= oneHourAgo);
            
            if (recentRequests >= apiKey.RateLimitPerHour.Value)
                return false;
        }

        return true;
    }

    public async Task LogApiKeyUsageAsync(int apiKeyId, string endpoint, string method, int responseStatus, int responseTime, string? ipAddress = null, string? userAgent = null, string? requestPayload = null)
    {
        var log = new AIAgentApiKeyUsageLog
        {
            ApiKeyId = apiKeyId,
            Endpoint = endpoint,
            Method = method,
            RequestPayload = requestPayload,
            ResponseStatus = responseStatus,
            ResponseTime = responseTime,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        _context.AIAgentApiKeyUsageLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    private string GenerateSecureApiKey()
    {
        // Generate a secure random key (64 characters)
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[48]; // 48 bytes = 64 base64 characters
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}

