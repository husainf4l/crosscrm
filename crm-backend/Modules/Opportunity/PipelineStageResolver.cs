using HotChocolate;
using HotChocolate.Data;
using crm_backend.Modules.Opportunity.DTOs;
using crm_backend.Modules.Opportunity.Services;
using crm_backend.Data;
using crm_backend.GraphQL;
using crm_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using FluentValidation;

namespace crm_backend.Modules.Opportunity;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class PipelineStageResolver : BaseResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<PipelineStageDto>> GetPipelineStages(
        [Service] IPipelineStageService pipelineStageService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdOrNullAsync(httpContextAccessor, context);
        
        if (!companyId.HasValue)
        {
            return new List<PipelineStageDto>();
        }

        return await pipelineStageService.GetAllPipelineStagesAsync(companyId.Value);
    }

    [Authorize]
    public async Task<PipelineStageDto?> GetPipelineStage(
        int id,
        [Service] IPipelineStageService pipelineStageService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var stage = await pipelineStageService.GetPipelineStageByIdAsync(id);

        if (stage == null || stage.CompanyId != companyId)
        {
            return null;
        }

        return stage;
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class PipelineStageMutation : BaseResolver
{
    [Authorize]
    public async Task<PipelineStageDto> CreatePipelineStage(
        CreatePipelineStageDto input,
        [Service] IPipelineStageService pipelineStageService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreatePipelineStageDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new GraphQLException($"Validation failed: {errors}");
            }

            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var modifiedInput = new CreatePipelineStageDto
            {
                Name = input.Name,
                Description = input.Description,
                Order = input.Order,
                DefaultProbability = input.DefaultProbability,
                IsActive = input.IsActive,
                IsWonStage = input.IsWonStage,
                IsLostStage = input.IsLostStage,
                CompanyId = companyId
            };

            return await pipelineStageService.CreatePipelineStageAsync(modifiedInput);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to create pipeline stage");
        }
    }

    [Authorize]
    public async Task<PipelineStageDto?> UpdatePipelineStage(
        int id,
        UpdatePipelineStageDto input,
        [Service] IPipelineStageService pipelineStageService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdatePipelineStageDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new GraphQLException($"Validation failed: {errors}");
            }

            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingStage = await pipelineStageService.GetPipelineStageByIdAsync(id);
            if (existingStage == null || existingStage.CompanyId != companyId)
            {
                throw new GraphQLException("Pipeline stage not found or access denied");
            }

            return await pipelineStageService.UpdatePipelineStageAsync(id, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to update pipeline stage");
        }
    }

    [Authorize]
    public async Task<bool> DeletePipelineStage(
        int id,
        [Service] IPipelineStageService pipelineStageService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingStage = await pipelineStageService.GetPipelineStageByIdAsync(id);
            if (existingStage == null || existingStage.CompanyId != companyId)
            {
                throw new GraphQLException("Pipeline stage not found or access denied");
            }

            return await pipelineStageService.DeletePipelineStageAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to delete pipeline stage");
        }
    }
}

