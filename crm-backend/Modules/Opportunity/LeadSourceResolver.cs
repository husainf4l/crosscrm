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
public class LeadSourceResolver : BaseResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<LeadSourceDto>> GetLeadSources(
        [Service] ILeadSourceService leadSourceService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdOrNullAsync(httpContextAccessor, context);
        
        if (!companyId.HasValue)
        {
            return new List<LeadSourceDto>();
        }

        return await leadSourceService.GetAllLeadSourcesAsync(companyId.Value);
    }

    [Authorize]
    public async Task<LeadSourceDto?> GetLeadSource(
        int id,
        [Service] ILeadSourceService leadSourceService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var source = await leadSourceService.GetLeadSourceByIdAsync(id);

        if (source == null || source.CompanyId != companyId)
        {
            return null;
        }

        return source;
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class LeadSourceMutation : BaseResolver
{
    [Authorize]
    public async Task<LeadSourceDto> CreateLeadSource(
        CreateLeadSourceDto input,
        [Service] ILeadSourceService leadSourceService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateLeadSourceDto> validator,
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

            var modifiedInput = new CreateLeadSourceDto
            {
                Name = input.Name,
                Description = input.Description,
                IsActive = input.IsActive,
                CompanyId = companyId
            };

            return await leadSourceService.CreateLeadSourceAsync(modifiedInput);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to create lead source");
        }
    }

    [Authorize]
    public async Task<LeadSourceDto?> UpdateLeadSource(
        int id,
        UpdateLeadSourceDto input,
        [Service] ILeadSourceService leadSourceService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateLeadSourceDto> validator,
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

            var existingSource = await leadSourceService.GetLeadSourceByIdAsync(id);
            if (existingSource == null || existingSource.CompanyId != companyId)
            {
                throw new GraphQLException("Lead source not found or access denied");
            }

            return await leadSourceService.UpdateLeadSourceAsync(id, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to update lead source");
        }
    }

    [Authorize]
    public async Task<bool> DeleteLeadSource(
        int id,
        [Service] ILeadSourceService leadSourceService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingSource = await leadSourceService.GetLeadSourceByIdAsync(id);
            if (existingSource == null || existingSource.CompanyId != companyId)
            {
                throw new GraphQLException("Lead source not found or access denied");
            }

            return await leadSourceService.DeleteLeadSourceAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to delete lead source");
        }
    }
}

