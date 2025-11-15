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
public class ActivityTimelineResolver : BaseResolver
{
    [Authorize]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<ActivityTimelineDto>> GetActivityTimeline(
        string entityType,
        int entityId,
        [Service] IActivityTimelineService activityService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var userId = GetUserId(httpContextAccessor.HttpContext);
        return await activityService.GetTimelineByEntityAsync(entityType, entityId, companyId, userId);
    }

    [Authorize]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<ActivityTimelineDto>> GetMyActivityFeed(
        [Service] IActivityTimelineService activityService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        int skip = 0,
        int take = 50)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var userId = GetUserId(httpContextAccessor.HttpContext);
        return await activityService.GetUserActivityFeedAsync(userId, companyId, skip, take);
    }

    [Authorize]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<ActivityTimelineDto>> GetActivitiesByType(
        string activityType,
        [Service] IActivityTimelineService activityService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        int skip = 0,
        int take = 50)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var userId = GetUserId(httpContextAccessor.HttpContext);
        return await activityService.GetActivitiesByTypeAsync(activityType, companyId, userId, skip, take);
    }

    [Authorize]
    public async Task<int> GetUnreadActivityCount(
        [Service] IActivityTimelineService activityService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var userId = GetUserId(httpContextAccessor.HttpContext);
        return await activityService.GetUnreadActivityCountAsync(userId, companyId);
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class ActivityTimelineMutation : BaseResolver
{
    [Authorize]
    public async Task<ActivityTimelineDto> LogActivity(
        CreateActivityTimelineDto input,
        [Service] IActivityTimelineService activityService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateActivityTimelineDto> validator,
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
            return await activityService.LogActivityAsync(input, companyId, userId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("LOG_ACTIVITY_ERROR", $"Failed to log activity: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<bool> MarkActivityAsRead(
        int activityId,
        [Service] IActivityTimelineService activityService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            var userId = GetUserId(httpContextAccessor.HttpContext);
            return await activityService.MarkActivityAsReadAsync(activityId, userId, companyId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("MARK_ACTIVITY_READ_ERROR", $"Failed to mark activity as read: {ex.Message}"));
        }
    }
}

