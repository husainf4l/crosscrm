using crm_backend.Data;
using crm_backend.GraphQL;
using crm_backend.Modules.User.DTOs;
using crm_backend.Modules.User.Services;
using crm_backend.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace crm_backend.Modules.User;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class UserInvitationResolver : BaseResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<UserInvitationDto>> GetCompanyInvitations(
        [Service] IUserInvitationService invitationService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        return await invitationService.GetCompanyInvitationsAsync(companyId);
    }

    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<UserInvitationDto>> GetMyInvitations(
        [Service] IUserInvitationService invitationService,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = GetUserId(httpContextAccessor.HttpContext);
        return await invitationService.GetUserInvitationsAsync(userId);
    }

    public async Task<UserInvitationDto?> GetInvitationByToken(
        string token,
        [Service] IUserInvitationService invitationService)
    {
        return await invitationService.GetInvitationByTokenAsync(token);
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class UserInvitationMutation : BaseResolver
{
    [Authorize]
    public async Task<UserInvitationDto> InviteUser(
        InviteUserDto input,
        [Service] IUserInvitationService invitationService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<InviteUserDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                throw new GraphQLException(errorHandling.CreateValidationError(validationResult));
            }

            var userId = GetUserId(httpContextAccessor.HttpContext);
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            // Ensure the user is inviting to their own company
            if (input.CompanyId != companyId)
            {
                throw new GraphQLException("You can only invite users to your own company");
            }

            return await invitationService.InviteUserAsync(input, userId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to invite user");
        }
    }

    public async Task<InvitationResponseDto> AcceptInvitation(
        AcceptInvitationDto input,
        [Service] IUserInvitationService invitationService,
        [Service] IValidator<AcceptInvitationDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                throw new GraphQLException(errorHandling.CreateValidationError(validationResult));
            }

            return await invitationService.AcceptInvitationAsync(input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to accept invitation");
        }
    }

    [Authorize]
    public async Task<bool> CancelInvitation(
        int invitationId,
        [Service] IUserInvitationService invitationService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var userId = GetUserId(httpContextAccessor.HttpContext);
            return await invitationService.CancelInvitationAsync(invitationId, userId);
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to cancel invitation");
        }
    }

    [Authorize]
    public async Task<bool> ResendInvitation(
        int invitationId,
        [Service] IUserInvitationService invitationService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var userId = GetUserId(httpContextAccessor.HttpContext);
            return await invitationService.ResendInvitationAsync(invitationId, userId);
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to resend invitation");
        }
    }
}
