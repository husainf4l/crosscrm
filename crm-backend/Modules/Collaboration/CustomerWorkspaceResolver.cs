using HotChocolate;
using HotChocolate.Data;
using crm_backend.Modules.Collaboration.DTOs;
using crm_backend.Modules.Collaboration.Services;
using crm_backend.Data;
using crm_backend.GraphQL;
using crm_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using FluentValidation;

namespace crm_backend.Modules.Collaboration;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class CustomerWorkspaceResolver : BaseResolver
{
    [Authorize]
    public async Task<CustomerWorkspaceDto?> GetCustomerWorkspace(
        int customerId,
        [Service] ICustomerWorkspaceService workspaceService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        return await workspaceService.GetWorkspaceByCustomerIdAsync(customerId, companyId);
    }

    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<CustomerStrategyDto>> GetCustomerStrategies(
        int customerId,
        [Service] ICustomerStrategyService strategyService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        return await strategyService.GetStrategiesByCustomerAsync(customerId, companyId);
    }

    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<CustomerIdeaDto>> GetCustomerIdeas(
        int customerId,
        [Service] ICustomerIdeaService ideaService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var userId = GetUserId(httpContextAccessor.HttpContext);
        return await ideaService.GetIdeasByCustomerAsync(customerId, companyId, userId);
    }

    [Authorize]
    public async Task<IEnumerable<NoteCommentDto>> GetEntityComments(
        string entityType,
        int entityId,
        [Service] INoteCommentService commentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        return await commentService.GetCommentsByEntityAsync(entityType, entityId, companyId);
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class CustomerWorkspaceMutation : BaseResolver
{
    [Authorize]
    public async Task<CustomerWorkspaceDto> CreateCustomerWorkspace(
        CreateCustomerWorkspaceDto input,
        [Service] ICustomerWorkspaceService workspaceService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateCustomerWorkspaceDto> validator,
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
            return await workspaceService.CreateWorkspaceAsync(input, companyId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("CREATE_WORKSPACE_ERROR", $"Failed to create workspace: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<CustomerWorkspaceDto?> UpdateCustomerWorkspace(
        int id,
        UpdateCustomerWorkspaceDto input,
        [Service] ICustomerWorkspaceService workspaceService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateCustomerWorkspaceDto> validator,
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
            return await workspaceService.UpdateWorkspaceAsync(id, input, companyId, userId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("UPDATE_WORKSPACE_ERROR", $"Failed to update workspace: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<CustomerStrategyDto> CreateCustomerStrategy(
        CreateCustomerStrategyDto input,
        [Service] ICustomerStrategyService strategyService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateCustomerStrategyDto> validator,
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
            return await strategyService.CreateStrategyAsync(input, companyId, userId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("CREATE_STRATEGY_ERROR", $"Failed to create strategy: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<CustomerStrategyDto?> UpdateCustomerStrategy(
        int id,
        UpdateCustomerStrategyDto input,
        [Service] ICustomerStrategyService strategyService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateCustomerStrategyDto> validator,
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
            return await strategyService.UpdateStrategyAsync(id, input, companyId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("UPDATE_STRATEGY_ERROR", $"Failed to update strategy: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<bool> DeleteCustomerStrategy(
        int id,
        [Service] ICustomerStrategyService strategyService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            return await strategyService.DeleteStrategyAsync(id, companyId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("DELETE_STRATEGY_ERROR", $"Failed to delete strategy: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<CustomerIdeaDto> CreateCustomerIdea(
        CreateCustomerIdeaDto input,
        [Service] ICustomerIdeaService ideaService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateCustomerIdeaDto> validator,
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
            return await ideaService.CreateIdeaAsync(input, companyId, userId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("CREATE_IDEA_ERROR", $"Failed to create idea: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<CustomerIdeaDto?> UpdateCustomerIdea(
        int id,
        UpdateCustomerIdeaDto input,
        [Service] ICustomerIdeaService ideaService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateCustomerIdeaDto> validator,
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
            return await ideaService.UpdateIdeaAsync(id, input, companyId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("UPDATE_IDEA_ERROR", $"Failed to update idea: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<CustomerIdeaDto> VoteCustomerIdea(
        VoteIdeaDto input,
        [Service] ICustomerIdeaService ideaService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<VoteIdeaDto> validator,
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
            return await ideaService.VoteIdeaAsync(input, userId, companyId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("VOTE_IDEA_ERROR", $"Failed to vote on idea: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<NoteCommentDto> CreateNoteComment(
        CreateNoteCommentDto input,
        [Service] INoteCommentService commentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateNoteCommentDto> validator,
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
            return await commentService.CreateCommentAsync(input, companyId, userId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("CREATE_COMMENT_ERROR", $"Failed to create comment: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<NoteCommentDto?> UpdateNoteComment(
        int id,
        UpdateNoteCommentDto input,
        [Service] INoteCommentService commentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateNoteCommentDto> validator,
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
            return await commentService.UpdateCommentAsync(id, input, companyId, userId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("UPDATE_COMMENT_ERROR", $"Failed to update comment: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<bool> DeleteNoteComment(
        int id,
        [Service] INoteCommentService commentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            var userId = GetUserId(httpContextAccessor.HttpContext);
            return await commentService.DeleteCommentAsync(id, companyId, userId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("DELETE_COMMENT_ERROR", $"Failed to delete comment: {ex.Message}"));
        }
    }
}

