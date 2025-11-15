using HotChocolate;
using HotChocolate.Data;
using crm_backend.Modules.Marketing.DTOs;
using crm_backend.Modules.Marketing.Services;
using crm_backend.Data;
using crm_backend.GraphQL;
using crm_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using FluentValidation;

namespace crm_backend.Modules.Marketing;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class CampaignResolver : BaseResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<CampaignDto>> GetCampaigns(
        [Service] ICampaignService campaignService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdOrNullAsync(httpContextAccessor, context);
        
        if (!companyId.HasValue)
        {
            return new List<CampaignDto>();
        }

        return await campaignService.GetAllCampaignsAsync(companyId.Value);
    }

    [Authorize]
    public async Task<CampaignDto?> GetCampaign(
        int id,
        [Service] ICampaignService campaignService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var campaign = await campaignService.GetCampaignByIdAsync(id);

        if (campaign == null || campaign.CompanyId != companyId)
        {
            return null;
        }

        return campaign;
    }

    [Authorize]
    public async Task<IEnumerable<CampaignMemberDto>> GetCampaignMembers(
        int campaignId,
        [Service] ICampaignService campaignService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

        // Verify campaign belongs to company
        var campaign = await campaignService.GetCampaignByIdAsync(campaignId);
        if (campaign == null || campaign.CompanyId != companyId)
        {
            return new List<CampaignMemberDto>();
        }

        return await campaignService.GetCampaignMembersAsync(campaignId);
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class CampaignMutation : BaseResolver
{
    [Authorize]
    public async Task<CampaignDto> CreateCampaign(
        CreateCampaignDto input,
        [Service] ICampaignService campaignService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateCampaignDto> validator,
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
            var userId = GetUserId(httpContextAccessor.HttpContext);

            var modifiedInput = new CreateCampaignDto
            {
                Name = input.Name,
                Description = input.Description,
                Type = input.Type,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                Budget = input.Budget,
                Currency = input.Currency,
                ExpectedLeads = input.ExpectedLeads,
                ExpectedRevenue = input.ExpectedRevenue,
                CompanyId = companyId,
                CreatedByUserId = userId
            };

            return await campaignService.CreateCampaignAsync(modifiedInput);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to create campaign");
        }
    }

    [Authorize]
    public async Task<CampaignDto?> UpdateCampaign(
        int id,
        UpdateCampaignDto input,
        [Service] ICampaignService campaignService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateCampaignDto> validator,
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

            var existingCampaign = await campaignService.GetCampaignByIdAsync(id);
            if (existingCampaign == null || existingCampaign.CompanyId != companyId)
            {
                throw new GraphQLException("Campaign not found or access denied");
            }

            return await campaignService.UpdateCampaignAsync(id, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to update campaign");
        }
    }

    [Authorize]
    public async Task<bool> DeleteCampaign(
        int id,
        [Service] ICampaignService campaignService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingCampaign = await campaignService.GetCampaignByIdAsync(id);
            if (existingCampaign == null || existingCampaign.CompanyId != companyId)
            {
                throw new GraphQLException("Campaign not found or access denied");
            }

            return await campaignService.DeleteCampaignAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to delete campaign");
        }
    }

    [Authorize]
    public async Task<CampaignMemberDto> AddCampaignMember(
        CreateCampaignMemberDto input,
        [Service] ICampaignService campaignService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            // Verify campaign belongs to company
            var campaign = await campaignService.GetCampaignByIdAsync(input.CampaignId);
            if (campaign == null || campaign.CompanyId != companyId)
            {
                throw new GraphQLException("Campaign not found or access denied");
            }

            return await campaignService.AddCampaignMemberAsync(input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to add campaign member");
        }
    }

    [Authorize]
    public async Task<bool> RemoveCampaignMember(
        int memberId,
        [Service] ICampaignService campaignService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            return await campaignService.RemoveCampaignMemberAsync(memberId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to remove campaign member");
        }
    }
}

