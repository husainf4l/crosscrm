using crm_backend.Data;
using crm_backend.GraphQL;
using crm_backend.Modules.Marketing.DTOs;
using crm_backend.Modules.Marketing.Services;
using crm_backend.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace crm_backend.Modules.Marketing;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class LeadResolver : BaseResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<LeadDto>> GetLeads(
        [Service] ILeadService leadService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdOrNullAsync(httpContextAccessor, context);

        if (!companyId.HasValue)
        {
            return new List<LeadDto>();
        }

        return await leadService.GetAllLeadsAsync(companyId.Value);
    }

    [Authorize]
    public async Task<LeadDto?> GetLead(
        int id,
        [Service] ILeadService leadService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var lead = await leadService.GetLeadByIdAsync(id);

        if (lead == null || lead.CompanyId != companyId)
        {
            return null;
        }

        return lead;
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class LeadMutation : BaseResolver
{
    [Authorize]
    public async Task<LeadDto> CreateLead(
        CreateLeadDto input,
        [Service] ILeadService leadService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateLeadDto> validator,
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

            var modifiedInput = new CreateLeadDto
            {
                FirstName = input.FirstName,
                LastName = input.LastName,
                CompanyName = input.CompanyName,
                Title = input.Title,
                Email = input.Email,
                Phone = input.Phone,
                Mobile = input.Mobile,
                Website = input.Website,
                Address = input.Address,
                City = input.City,
                State = input.State,
                Country = input.Country,
                PostalCode = input.PostalCode,
                Industry = input.Industry,
                EstimatedValue = input.EstimatedValue,
                Currency = input.Currency,
                Rating = input.Rating,
                SourceId = input.SourceId,
                AssignedUserId = input.AssignedUserId,
                CompanyId = companyId
            };

            return await leadService.CreateLeadAsync(modifiedInput);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to create lead");
        }
    }

    [Authorize]
    public async Task<LeadDto?> UpdateLead(
        int id,
        UpdateLeadDto input,
        [Service] ILeadService leadService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateLeadDto> validator,
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

            var existingLead = await leadService.GetLeadByIdAsync(id);
            if (existingLead == null || existingLead.CompanyId != companyId)
            {
                throw new GraphQLException("Lead not found or access denied");
            }

            return await leadService.UpdateLeadAsync(id, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to update lead");
        }
    }

    [Authorize]
    public async Task<bool> DeleteLead(
        int id,
        [Service] ILeadService leadService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingLead = await leadService.GetLeadByIdAsync(id);
            if (existingLead == null || existingLead.CompanyId != companyId)
            {
                throw new GraphQLException("Lead not found or access denied");
            }

            return await leadService.DeleteLeadAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to delete lead");
        }
    }

    [Authorize]
    public async Task<LeadDto> ConvertLeadToCustomer(
        int leadId,
        ConvertLeadDto? input,
        [Service] ILeadService leadService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingLead = await leadService.GetLeadByIdAsync(leadId);
            if (existingLead == null || existingLead.CompanyId != companyId)
            {
                throw new GraphQLException("Lead not found or access denied");
            }

            return await leadService.ConvertLeadToCustomerAsync(leadId, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to convert lead to customer");
        }
    }

    [Authorize]
    public async Task<LeadDto> ConvertLeadToOpportunity(
        int leadId,
        ConvertLeadDto? input,
        [Service] ILeadService leadService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingLead = await leadService.GetLeadByIdAsync(leadId);
            if (existingLead == null || existingLead.CompanyId != companyId)
            {
                throw new GraphQLException("Lead not found or access denied");
            }

            return await leadService.ConvertLeadToOpportunityAsync(leadId, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to convert lead to opportunity");
        }
    }
}

