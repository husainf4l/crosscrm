using crm_backend.Data;
using crm_backend.GraphQL;
using crm_backend.Modules.Opportunity.DTOs;
using crm_backend.Modules.Opportunity.Services;
using crm_backend.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace crm_backend.Modules.Opportunity;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class OpportunityResolver : BaseResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<OpportunityDto>> GetOpportunities(
        [Service] IOpportunityService opportunityService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdOrNullAsync(httpContextAccessor, context);

        if (!companyId.HasValue)
        {
            return new List<OpportunityDto>();
        }

        return await opportunityService.GetAllOpportunitiesAsync(companyId.Value);
    }

    [Authorize]
    public async Task<OpportunityDto?> GetOpportunity(
        int id,
        [Service] IOpportunityService opportunityService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var opportunity = await opportunityService.GetOpportunityByIdAsync(id);

        if (opportunity == null || opportunity.CompanyId != companyId)
        {
            return null;
        }

        return opportunity;
    }

    [Authorize]
    public async Task<decimal> GetWeightedPipelineValue(
        [Service] IOpportunityService opportunityService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        return await opportunityService.CalculateWeightedPipelineValueAsync(companyId);
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class OpportunityMutation : BaseResolver
{
    [Authorize]
    public async Task<OpportunityDto> CreateOpportunity(
        CreateOpportunityDto input,
        [Service] IOpportunityService opportunityService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateOpportunityDto> validator,
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

            var modifiedInput = new CreateOpportunityDto
            {
                Name = input.Name,
                Description = input.Description,
                Amount = input.Amount,
                Currency = input.Currency,
                Probability = input.Probability,
                PipelineStageId = input.PipelineStageId,
                ExpectedCloseDate = input.ExpectedCloseDate,
                CustomerId = input.CustomerId,
                AssignedUserId = input.AssignedUserId,
                SourceId = input.SourceId,
                CompanyId = companyId
            };

            return await opportunityService.CreateOpportunityAsync(modifiedInput);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to create opportunity");
        }
    }

    [Authorize]
    public async Task<OpportunityDto?> UpdateOpportunity(
        int id,
        UpdateOpportunityDto input,
        [Service] IOpportunityService opportunityService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateOpportunityDto> validator,
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

            var existingOpportunity = await opportunityService.GetOpportunityByIdAsync(id);
            if (existingOpportunity == null || existingOpportunity.CompanyId != companyId)
            {
                throw new GraphQLException("Opportunity not found or access denied");
            }

            return await opportunityService.UpdateOpportunityAsync(id, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to update opportunity");
        }
    }

    [Authorize]
    public async Task<bool> DeleteOpportunity(
        int id,
        [Service] IOpportunityService opportunityService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingOpportunity = await opportunityService.GetOpportunityByIdAsync(id);
            if (existingOpportunity == null || existingOpportunity.CompanyId != companyId)
            {
                throw new GraphQLException("Opportunity not found or access denied");
            }

            return await opportunityService.DeleteOpportunityAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to delete opportunity");
        }
    }

    [Authorize]
    public async Task<OpportunityDto?> MoveOpportunityToStage(
        int id,
        int pipelineStageId,
        [Service] IOpportunityService opportunityService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingOpportunity = await opportunityService.GetOpportunityByIdAsync(id);
            if (existingOpportunity == null || existingOpportunity.CompanyId != companyId)
            {
                throw new GraphQLException("Opportunity not found or access denied");
            }

            return await opportunityService.MoveOpportunityToStageAsync(id, pipelineStageId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to move opportunity to stage");
        }
    }
}

