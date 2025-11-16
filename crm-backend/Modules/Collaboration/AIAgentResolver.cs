using crm_backend.Data;
using crm_backend.GraphQL;
using crm_backend.Modules.Collaboration.DTOs;
using crm_backend.Modules.Collaboration.Services;
using crm_backend.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Collaboration;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class AIAgentResolver : BaseResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<AIAgentDto>> GetAIAgents(
        [Service] IAIAgentService agentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdOrNullAsync(httpContextAccessor, context);

        if (!companyId.HasValue)
        {
            return new List<AIAgentDto>();
        }

        return await agentService.GetAllAgentsAsync(companyId.Value);
    }

    [Authorize]
    public async Task<AIAgentDto?> GetAIAgent(
        int id,
        [Service] IAIAgentService agentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var agent = await agentService.GetAgentByIdAsync(id);

        if (agent == null || agent.CompanyId != companyId)
        {
            return null;
        }

        return agent;
    }

    [Authorize]
    public async Task<IEnumerable<AIAgentAssignmentDto>> GetAIAgentAssignments(
        int agentId,
        [Service] IAIAgentService agentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

        // Verify agent belongs to company
        var agent = await agentService.GetAgentByIdAsync(agentId);
        if (agent == null || agent.CompanyId != companyId)
        {
            return new List<AIAgentAssignmentDto>();
        }

        return await agentService.GetAgentAssignmentsAsync(agentId);
    }

    [Authorize]
    public async Task<IEnumerable<AIAgentAssignmentDto>> GetAIAgentAssignmentsByEntity(
        string entityType,
        int entityId,
        [Service] IAIAgentService agentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        return await agentService.GetAssignmentsByEntityAsync(entityType, entityId, companyId);
    }

    [Authorize]
    public async Task<IEnumerable<AIAgentInteractionDto>> GetAIAgentInteractions(
        int agentId,
        [Service] IAIAgentInteractionService interactionService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        return await interactionService.GetInteractionsByAgentAsync(agentId, companyId);
    }

    [Authorize]
    public async Task<IEnumerable<AIAgentInteractionDto>> GetAIAgentInteractionsByEntity(
        string entityType,
        int entityId,
        [Service] IAIAgentInteractionService interactionService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        return await interactionService.GetInteractionsByEntityAsync(entityType, entityId, companyId);
    }

    [Authorize]
    public async Task<IEnumerable<AIAgentInteractionDto>> GetMyAIAgentInteractions(
        [Service] IAIAgentInteractionService interactionService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var userId = GetUserId(httpContextAccessor.HttpContext);
        return await interactionService.GetInteractionsByUserAsync(userId, companyId);
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class AIAgentMutation : BaseResolver
{
    [Authorize]
    public async Task<AIAgentDto> CreateAIAgent(
        CreateAIAgentDto input,
        [Service] IAIAgentService agentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateAIAgentDto> validator,
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

            return await agentService.CreateAgentAsync(input, companyId, userId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("CREATE_AI_AGENT_ERROR", $"Failed to create AI agent: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<AIAgentDto?> UpdateAIAgent(
        int id,
        UpdateAIAgentDto input,
        [Service] IAIAgentService agentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateAIAgentDto> validator,
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

            // Verify agent belongs to company
            var existingAgent = await agentService.GetAgentByIdAsync(id);
            if (existingAgent == null || existingAgent.CompanyId != companyId)
            {
                throw new GraphQLException(errorHandling.CreateError("AGENT_NOT_FOUND", "AI agent not found or access denied."));
            }

            return await agentService.UpdateAgentAsync(id, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("UPDATE_AI_AGENT_ERROR", $"Failed to update AI agent: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<bool> DeleteAIAgent(
        int id,
        [Service] IAIAgentService agentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            // Verify agent belongs to company
            var existingAgent = await agentService.GetAgentByIdAsync(id);
            if (existingAgent == null || existingAgent.CompanyId != companyId)
            {
                throw new GraphQLException(errorHandling.CreateError("AGENT_NOT_FOUND", "AI agent not found or access denied."));
            }

            return await agentService.DeleteAgentAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("DELETE_AI_AGENT_ERROR", $"Failed to delete AI agent: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<AIAgentAssignmentDto> AssignAIAgent(
        CreateAIAgentAssignmentDto input,
        [Service] IAIAgentService agentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateAIAgentAssignmentDto> validator,
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

            return await agentService.AssignAgentAsync(input, companyId, userId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("ASSIGN_AI_AGENT_ERROR", $"Failed to assign AI agent: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<bool> UnassignAIAgent(
        int assignmentId,
        [Service] IAIAgentService agentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            // Verify assignment belongs to company (check through agent)
            var assignment = await context.AIAgentAssignments
                .Include(aa => aa.Agent)
                .FirstOrDefaultAsync(aa => aa.Id == assignmentId);

            if (assignment == null || assignment.CompanyId != companyId)
            {
                throw new GraphQLException(errorHandling.CreateError("ASSIGNMENT_NOT_FOUND", "Assignment not found or access denied."));
            }

            return await agentService.UnassignAgentAsync(assignmentId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("UNASSIGN_AI_AGENT_ERROR", $"Failed to unassign AI agent: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<AIAgentInteractionDto> CreateAIAgentInteraction(
        CreateAIAgentInteractionDto input,
        [Service] IAIAgentInteractionService interactionService,
        [Service] IAIAgentClientService clientService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateAIAgentInteractionDto> validator,
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

            // Create interaction record
            var interaction = await interactionService.CreateInteractionAsync(input, companyId, userId);

            // Send request to Python service asynchronously (fire and forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    var response = await clientService.SendRequestAsync(
                        input.AgentId,
                        input.Input,
                        input.EntityType,
                        input.EntityId,
                        userId);

                    // Update interaction with response
                    await interactionService.UpdateInteractionAsync(interaction.Id, new UpdateAIAgentInteractionDto
                    {
                        Output = response,
                        Status = "Success"
                    });
                }
                catch (Exception ex)
                {
                    // Update interaction with error
                    await interactionService.UpdateInteractionAsync(interaction.Id, new UpdateAIAgentInteractionDto
                    {
                        Output = $"Error: {ex.Message}",
                        Status = "Failed"
                    });
                }
            });

            return interaction;
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("CREATE_AI_AGENT_INTERACTION_ERROR", $"Failed to create AI agent interaction: {ex.Message}"));
        }
    }
}

