using System.Diagnostics;
using System.Text.Json;
using crm_backend.Data;
using crm_backend.Modules.Collaboration;
using crm_backend.Modules.Collaboration.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Collaboration.Services;

public class AIAgentToolService : IAIAgentToolService
{
    private readonly CrmDbContext _context;
    private readonly IServiceProvider _serviceProvider;

    public AIAgentToolService(CrmDbContext context, IServiceProvider serviceProvider)
    {
        _context = context;
        _serviceProvider = serviceProvider;
    }

    public async Task<IEnumerable<AIAgentToolDto>> GetToolsByAgentAsync(int agentId, int companyId)
    {
        var tools = await _context.AIAgentTools
            .Include(t => t.Agent)
            .Include(t => t.CreatedByUser)
            .Where(t => t.AgentId == agentId && t.CompanyId == companyId)
            .Select(t => new AIAgentToolDto
            {
                Id = t.Id,
                AgentId = t.AgentId,
                AgentName = t.Agent.Name,
                ToolName = t.ToolName,
                DisplayName = t.DisplayName,
                Description = t.Description,
                Type = t.Type.ToString(),
                Endpoint = t.Endpoint,
                Method = t.Method,
                Parameters = !string.IsNullOrEmpty(t.Parameters)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(t.Parameters)
                    : null,
                Permissions = !string.IsNullOrEmpty(t.Permissions)
                    ? JsonSerializer.Deserialize<List<string>>(t.Permissions) ?? new List<string>()
                    : new List<string>(),
                IsActive = t.IsActive,
                CreatedByUserId = t.CreatedByUserId,
                CreatedByUserName = t.CreatedByUser.Name,
                CompanyId = t.CompanyId,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync();

        return tools;
    }

    public async Task<AIAgentToolDto?> GetToolByIdAsync(int id)
    {
        var tool = await _context.AIAgentTools
            .Include(t => t.Agent)
            .Include(t => t.CreatedByUser)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tool == null) return null;

        return new AIAgentToolDto
        {
            Id = tool.Id,
            AgentId = tool.AgentId,
            AgentName = tool.Agent.Name,
            ToolName = tool.ToolName,
            DisplayName = tool.DisplayName,
            Description = tool.Description,
            Type = tool.Type.ToString(),
            Endpoint = tool.Endpoint,
            Method = tool.Method,
            Parameters = !string.IsNullOrEmpty(tool.Parameters)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(tool.Parameters)
                : null,
            Permissions = !string.IsNullOrEmpty(tool.Permissions)
                ? JsonSerializer.Deserialize<List<string>>(tool.Permissions) ?? new List<string>()
                : new List<string>(),
            IsActive = tool.IsActive,
            CreatedByUserId = tool.CreatedByUserId,
            CreatedByUserName = tool.CreatedByUser.Name,
            CompanyId = tool.CompanyId,
            CreatedAt = tool.CreatedAt,
            UpdatedAt = tool.UpdatedAt
        };
    }

    public async Task<AIAgentToolDto?> GetToolByNameAsync(int agentId, string toolName, int companyId)
    {
        var tool = await _context.AIAgentTools
            .Include(t => t.Agent)
            .Include(t => t.CreatedByUser)
            .FirstOrDefaultAsync(t => t.AgentId == agentId && t.ToolName == toolName && t.CompanyId == companyId);

        if (tool == null) return null;

        return new AIAgentToolDto
        {
            Id = tool.Id,
            AgentId = tool.AgentId,
            AgentName = tool.Agent.Name,
            ToolName = tool.ToolName,
            DisplayName = tool.DisplayName,
            Description = tool.Description,
            Type = tool.Type.ToString(),
            Endpoint = tool.Endpoint,
            Method = tool.Method,
            Parameters = !string.IsNullOrEmpty(tool.Parameters)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(tool.Parameters)
                : null,
            Permissions = !string.IsNullOrEmpty(tool.Permissions)
                ? JsonSerializer.Deserialize<List<string>>(tool.Permissions) ?? new List<string>()
                : new List<string>(),
            IsActive = tool.IsActive,
            CreatedByUserId = tool.CreatedByUserId,
            CreatedByUserName = tool.CreatedByUser.Name,
            CompanyId = tool.CompanyId,
            CreatedAt = tool.CreatedAt,
            UpdatedAt = tool.UpdatedAt
        };
    }

    public async Task<AIAgentToolDto> CreateToolAsync(CreateAIAgentToolDto dto, int companyId, int createdByUserId)
    {
        // Verify agent exists and belongs to company
        var agent = await _context.AIAgents
            .FirstOrDefaultAsync(a => a.Id == dto.AgentId && a.CompanyId == companyId);
        if (agent == null)
        {
            throw new InvalidOperationException("Agent not found or does not belong to the company.");
        }

        // Check if tool name already exists for this agent
        var existingTool = await _context.AIAgentTools
            .FirstOrDefaultAsync(t => t.AgentId == dto.AgentId && t.ToolName == dto.ToolName);
        if (existingTool != null)
        {
            throw new InvalidOperationException($"Tool with name '{dto.ToolName}' already exists for this agent.");
        }

        // Parse tool type
        if (!Enum.TryParse<ToolType>(dto.Type, true, out var toolType))
        {
            throw new ArgumentException($"Invalid tool type: {dto.Type}");
        }

        var tool = new AIAgentTool
        {
            AgentId = dto.AgentId,
            ToolName = dto.ToolName,
            DisplayName = dto.DisplayName,
            Description = dto.Description,
            Type = toolType,
            Endpoint = dto.Endpoint,
            Method = dto.Method.ToUpper(),
            Parameters = dto.Parameters != null ? JsonSerializer.Serialize(dto.Parameters) : "{}",
            Permissions = dto.Permissions != null ? JsonSerializer.Serialize(dto.Permissions) : "[]",
            IsActive = true,
            CreatedByUserId = createdByUserId,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.AIAgentTools.Add(tool);
        await _context.SaveChangesAsync();

        return await GetToolByIdAsync(tool.Id) 
            ?? throw new InvalidOperationException("Failed to retrieve created tool");
    }

    public async Task<AIAgentToolDto?> UpdateToolAsync(int id, UpdateAIAgentToolDto dto, int companyId)
    {
        var tool = await _context.AIAgentTools
            .FirstOrDefaultAsync(t => t.Id == id && t.CompanyId == companyId);
        if (tool == null) return null;

        if (!string.IsNullOrEmpty(dto.DisplayName))
            tool.DisplayName = dto.DisplayName;

        if (dto.Description != null)
            tool.Description = dto.Description;

        if (!string.IsNullOrEmpty(dto.Type) && Enum.TryParse<ToolType>(dto.Type, true, out var toolType))
            tool.Type = toolType;

        if (!string.IsNullOrEmpty(dto.Endpoint))
            tool.Endpoint = dto.Endpoint;

        if (!string.IsNullOrEmpty(dto.Method))
            tool.Method = dto.Method.ToUpper();

        if (dto.Parameters != null)
            tool.Parameters = JsonSerializer.Serialize(dto.Parameters);

        if (dto.Permissions != null)
            tool.Permissions = JsonSerializer.Serialize(dto.Permissions);

        if (dto.IsActive.HasValue)
            tool.IsActive = dto.IsActive.Value;

        tool.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetToolByIdAsync(tool.Id);
    }

    public async Task<bool> DeleteToolAsync(int id, int companyId)
    {
        var tool = await _context.AIAgentTools
            .FirstOrDefaultAsync(t => t.Id == id && t.CompanyId == companyId);
        if (tool == null) return false;

        _context.AIAgentTools.Remove(tool);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ExecuteToolResponseDto> ExecuteToolAsync(string toolName, Dictionary<string, object>? parameters, int apiKeyId, int companyId)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Get API key
            var apiKey = await _context.AIAgentApiKeys
                .Include(ak => ak.Agent)
                .FirstOrDefaultAsync(ak => ak.Id == apiKeyId);
            if (apiKey == null || apiKey.CompanyId != companyId || !apiKey.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid API key.");
            }

            // Get tool
            var tool = await _context.AIAgentTools
                .FirstOrDefaultAsync(t => t.AgentId == apiKey.AgentId && t.ToolName == toolName && t.CompanyId == companyId);
            if (tool == null || !tool.IsActive)
            {
                throw new InvalidOperationException($"Tool '{toolName}' not found or is not active.");
            }

            // Check permissions
            var toolPermissions = !string.IsNullOrEmpty(tool.Permissions)
                ? JsonSerializer.Deserialize<List<string>>(tool.Permissions) ?? new List<string>()
                : new List<string>();

            var apiKeyPermissions = !string.IsNullOrEmpty(apiKey.Permissions)
                ? JsonSerializer.Deserialize<List<string>>(apiKey.Permissions) ?? new List<string>()
                : new List<string>();

            // If API key has empty permissions array, it's a master key (all permissions)
            // Otherwise, check if API key has all required permissions
            if (apiKeyPermissions.Count > 0 && toolPermissions.Count > 0)
            {
                var missingPermissions = toolPermissions.Except(apiKeyPermissions, StringComparer.OrdinalIgnoreCase).ToList();
                if (missingPermissions.Any())
                {
                    throw new UnauthorizedAccessException($"API key missing required permissions: {string.Join(", ", missingPermissions)}");
                }
            }

            // Execute tool - for now, we'll return a placeholder
            // In a full implementation, this would route to the appropriate service/endpoint
            // For now, we'll just log and return success
            var result = new { message = "Tool executed successfully", toolName = tool.ToolName, parameters = parameters };

            stopwatch.Stop();

            // Log tool usage
            await LogToolUsageAsync(
                tool.Id,
                apiKeyId,
                parameters,
                result,
                ToolExecutionStatus.Success,
                (int)stopwatch.ElapsedMilliseconds
            );

            return new ExecuteToolResponseDto
            {
                Success = true,
                Result = result,
                ExecutionTime = (int)stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Try to get tool and API key for logging
            var apiKey = await _context.AIAgentApiKeys.FindAsync(apiKeyId);
            var tool = apiKey != null
                ? await _context.AIAgentTools.FirstOrDefaultAsync(t => t.AgentId == apiKey.AgentId && t.ToolName == toolName)
                : null;

            if (tool != null && apiKey != null)
            {
                await LogToolUsageAsync(
                    tool.Id,
                    apiKeyId,
                    parameters,
                    null,
                    ToolExecutionStatus.Error,
                    (int)stopwatch.ElapsedMilliseconds,
                    ex.Message
                );
            }

            return new ExecuteToolResponseDto
            {
                Success = false,
                ErrorMessage = ex.Message,
                ExecutionTime = (int)stopwatch.ElapsedMilliseconds
            };
        }
    }

    public async Task LogToolUsageAsync(int toolId, int apiKeyId, Dictionary<string, object>? parameters, object? result, ToolExecutionStatus status, int executionTime, string? errorMessage = null)
    {
        var log = new AIAgentToolUsageLog
        {
            ToolId = toolId,
            ApiKeyId = apiKeyId,
            Parameters = parameters != null ? JsonSerializer.Serialize(parameters) : "{}",
            Result = result != null ? JsonSerializer.Serialize(result) : null,
            Status = status,
            ErrorMessage = errorMessage,
            ExecutionTime = executionTime,
            CreatedAt = DateTime.UtcNow
        };

        _context.AIAgentToolUsageLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}

