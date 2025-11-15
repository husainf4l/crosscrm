using HotChocolate;
using HotChocolate.Data;
using crm_backend.Modules.Collaboration.DTOs;
using crm_backend.Modules.Collaboration.Services;
using crm_backend.Data;
using crm_backend.GraphQL;
using crm_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace crm_backend.Modules.Collaboration;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class AIAgentApiKeyResolver : BaseResolver
{
    [Authorize]
    public async Task<IEnumerable<AIAgentApiKeyDto>> GetAIAgentApiKeys(
        int agentId,
        [Service] IAIAgentApiKeyService apiKeyService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

        // Verify agent belongs to company
        var agent = await context.AIAgents
            .FirstOrDefaultAsync(a => a.Id == agentId && a.CompanyId == companyId);
        if (agent == null)
        {
            return new List<AIAgentApiKeyDto>();
        }

        return await apiKeyService.GetApiKeysByAgentAsync(agentId, companyId);
    }

    [Authorize]
    public async Task<AIAgentApiKeyDto?> GetAIAgentApiKey(
        int id,
        [Service] IAIAgentApiKeyService apiKeyService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var apiKey = await apiKeyService.GetApiKeyByIdAsync(id);

        if (apiKey == null || apiKey.CompanyId != companyId)
        {
            return null;
        }

        return apiKey;
    }

    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<AIAgentToolDto>> GetAIAgentTools(
        int agentId,
        [Service] IAIAgentToolService toolService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        return await toolService.GetToolsByAgentAsync(agentId, companyId);
    }

    [Authorize]
    public async Task<AIAgentToolDto?> GetAIAgentTool(
        int id,
        [Service] IAIAgentToolService toolService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var tool = await toolService.GetToolByIdAsync(id);

        if (tool == null || tool.CompanyId != companyId)
        {
            return null;
        }

        return tool;
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class AIAgentApiKeyMutation : BaseResolver
{
    [Authorize]
    public async Task<CreateAIAgentApiKeyResponseDto> GenerateAIAgentApiKey(
        CreateAIAgentApiKeyDto input,
        [Service] IAIAgentApiKeyService apiKeyService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateAIAgentApiKeyDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                throw new GraphQLException(errorHandling.CreateValidationError(validationResult));
            }

            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            var userId = GetUserId(httpContextAccessor.HttpContext);
            return await apiKeyService.GenerateApiKeyAsync(input, companyId, userId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("GENERATE_API_KEY_ERROR", $"Failed to generate API key: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<AIAgentApiKeyDto?> UpdateAIAgentApiKey(
        int id,
        UpdateAIAgentApiKeyDto input,
        [Service] IAIAgentApiKeyService apiKeyService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateAIAgentApiKeyDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                throw new GraphQLException(errorHandling.CreateValidationError(validationResult));
            }

            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            return await apiKeyService.UpdateApiKeyAsync(id, input, companyId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("UPDATE_API_KEY_ERROR", $"Failed to update API key: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<bool> RevokeAIAgentApiKey(
        int id,
        [Service] IAIAgentApiKeyService apiKeyService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            return await apiKeyService.RevokeApiKeyAsync(id, companyId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("REVOKE_API_KEY_ERROR", $"Failed to revoke API key: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<AIAgentToolDto> CreateAIAgentTool(
        CreateAIAgentToolDto input,
        [Service] IAIAgentToolService toolService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateAIAgentToolDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                throw new GraphQLException(errorHandling.CreateValidationError(validationResult));
            }

            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            var userId = GetUserId(httpContextAccessor.HttpContext);
            return await toolService.CreateToolAsync(input, companyId, userId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("CREATE_TOOL_ERROR", $"Failed to create tool: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<AIAgentToolDto?> UpdateAIAgentTool(
        int id,
        UpdateAIAgentToolDto input,
        [Service] IAIAgentToolService toolService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateAIAgentToolDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                throw new GraphQLException(errorHandling.CreateValidationError(validationResult));
            }

            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            return await toolService.UpdateToolAsync(id, input, companyId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("UPDATE_TOOL_ERROR", $"Failed to update tool: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<bool> DeleteAIAgentTool(
        int id,
        [Service] IAIAgentToolService toolService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            return await toolService.DeleteToolAsync(id, companyId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("DELETE_TOOL_ERROR", $"Failed to delete tool: {ex.Message}"));
        }
    }
}

