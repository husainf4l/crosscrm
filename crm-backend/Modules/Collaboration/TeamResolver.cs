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
public class TeamResolver : BaseResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<TeamDto>> GetTeams(
        [Service] ITeamService teamService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdOrNullAsync(httpContextAccessor, context);
        
        if (!companyId.HasValue)
        {
            return new List<TeamDto>();
        }

        return await teamService.GetAllTeamsAsync(companyId.Value);
    }

    [Authorize]
    public async Task<TeamDto?> GetTeam(
        int id,
        [Service] ITeamService teamService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var team = await teamService.GetTeamByIdAsync(id);

        if (team == null || team.CompanyId != companyId)
        {
            return null;
        }

        return team;
    }

    [Authorize]
    public async Task<IEnumerable<TeamDto>> GetMyTeams(
        [Service] ITeamService teamService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var userId = GetUserId(httpContextAccessor.HttpContext);
        
        return await teamService.GetTeamsByUserAsync(userId, companyId);
    }

    [Authorize]
    public async Task<IEnumerable<TeamMemberDto>> GetTeamMembers(
        int teamId,
        [Service] ITeamService teamService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

        // Verify team belongs to company
        var team = await teamService.GetTeamByIdAsync(teamId);
        if (team == null || team.CompanyId != companyId)
        {
            return new List<TeamMemberDto>();
        }

        return await teamService.GetTeamMembersAsync(teamId);
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class TeamMutation : BaseResolver
{
    [Authorize]
    public async Task<TeamDto> CreateTeam(
        CreateTeamDto input,
        [Service] ITeamService teamService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateTeamDto> validator,
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

            var modifiedInput = new CreateTeamDto
            {
                Name = input.Name,
                Description = input.Description,
                Type = input.Type,
                ManagerUserId = input.ManagerUserId,
                CompanyId = companyId,
                MemberUserIds = input.MemberUserIds
            };

            return await teamService.CreateTeamAsync(modifiedInput);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to create team");
        }
    }

    [Authorize]
    public async Task<TeamDto?> UpdateTeam(
        int id,
        UpdateTeamDto input,
        [Service] ITeamService teamService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateTeamDto> validator,
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

            var existingTeam = await teamService.GetTeamByIdAsync(id);
            if (existingTeam == null || existingTeam.CompanyId != companyId)
            {
                throw new GraphQLException("Team not found or access denied");
            }

            return await teamService.UpdateTeamAsync(id, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to update team");
        }
    }

    [Authorize]
    public async Task<bool> DeleteTeam(
        int id,
        [Service] ITeamService teamService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingTeam = await teamService.GetTeamByIdAsync(id);
            if (existingTeam == null || existingTeam.CompanyId != companyId)
            {
                throw new GraphQLException("Team not found or access denied");
            }

            return await teamService.DeleteTeamAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to delete team");
        }
    }

    [Authorize]
    public async Task<TeamMemberDto> AddTeamMember(
        AddTeamMemberDto input,
        [Service] ITeamService teamService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            // Verify team belongs to company
            var team = await teamService.GetTeamByIdAsync(input.TeamId);
            if (team == null || team.CompanyId != companyId)
            {
                throw new GraphQLException("Team not found or access denied");
            }

            return await teamService.AddTeamMemberAsync(input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to add team member");
        }
    }

    [Authorize]
    public async Task<TeamMemberDto?> UpdateTeamMember(
        int memberId,
        UpdateTeamMemberDto input,
        [Service] ITeamService teamService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            return await teamService.UpdateTeamMemberAsync(memberId, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to update team member");
        }
    }

    [Authorize]
    public async Task<bool> RemoveTeamMember(
        int memberId,
        [Service] ITeamService teamService,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            return await teamService.RemoveTeamMemberAsync(memberId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to remove team member");
        }
    }
}

