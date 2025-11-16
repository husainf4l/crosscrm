using crm_backend.Data;
using crm_backend.GraphQL;
using crm_backend.Modules.Collaboration.DTOs;
using crm_backend.Modules.Collaboration.Services;
using crm_backend.Services;
using FluentValidation;
using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using Microsoft.AspNetCore.Authorization;

namespace crm_backend.Modules.Collaboration;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class NotificationResolver : BaseResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<NotificationDto>> GetNotifications(
        [Service] INotificationService notificationService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        bool? unreadOnly = null,
        int? limit = null)
    {
        var userId = GetUserId(httpContextAccessor.HttpContext);
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        return await notificationService.GetNotificationsForUserAsync(userId, companyId, unreadOnly, limit);
    }

    [Authorize]
    public async Task<NotificationDto?> GetNotification(
        int id,
        [Service] INotificationService notificationService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var userId = GetUserId(httpContextAccessor.HttpContext);
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        return await notificationService.GetNotificationByIdAsync(id, userId, companyId);
    }

    [Authorize]
    public async Task<NotificationCountDto> GetNotificationCount(
        [Service] INotificationService notificationService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var userId = GetUserId(httpContextAccessor.HttpContext);
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        return await notificationService.GetNotificationCountAsync(userId, companyId);
    }

    [Authorize]
    public async Task<IEnumerable<NotificationPreferenceDto>> GetNotificationPreferences(
        [Service] INotificationService notificationService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var userId = GetUserId(httpContextAccessor.HttpContext);
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        return await notificationService.GetNotificationPreferencesAsync(userId, companyId);
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class NotificationMutation : BaseResolver
{
    [Authorize]
    public async Task<NotificationDto> CreateNotification(
        CreateNotificationDto input,
        [Service] INotificationService notificationService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateNotificationDto> validator,
        [Service] IErrorHandlingService errorHandling,
        [Service] ITopicEventSender eventSender)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                throw new GraphQLException(errorHandling.CreateValidationError(validationResult));
            }

            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            var notification = await notificationService.CreateNotificationAsync(input, companyId);

            // Publish notification event for real-time updates
            // Topic format: notification_{userId}
            await eventSender.SendAsync(
                $"notification_{input.UserId}",
                notification,
                default);

            return notification;
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("CREATE_NOTIFICATION_ERROR", $"Failed to create notification: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<bool> MarkNotificationAsRead(
        int id,
        [Service] INotificationService notificationService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling,
        [Service] ITopicEventSender eventSender)
    {
        try
        {
            var userId = GetUserId(httpContextAccessor.HttpContext);
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            var result = await notificationService.MarkNotificationAsReadAsync(id, userId, companyId);

            if (result)
            {
                // Publish update event
                var count = await notificationService.GetNotificationCountAsync(userId, companyId);
                await eventSender.SendAsync(
                    $"notification_count_{userId}",
                    count,
                    default);
            }

            return result;
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("MARK_NOTIFICATION_READ_ERROR", $"Failed to mark notification as read: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<int> MarkAllNotificationsAsRead(
        [Service] INotificationService notificationService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling,
        [Service] ITopicEventSender eventSender)
    {
        try
        {
            var userId = GetUserId(httpContextAccessor.HttpContext);
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            var count = await notificationService.MarkAllNotificationsAsReadAsync(userId, companyId);

            // Publish update event
            var notificationCount = await notificationService.GetNotificationCountAsync(userId, companyId);
            await eventSender.SendAsync(
                $"notification_count_{userId}",
                notificationCount,
                default);

            return count;
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("MARK_ALL_NOTIFICATIONS_READ_ERROR", $"Failed to mark all notifications as read: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<bool> DeleteNotification(
        int id,
        [Service] INotificationService notificationService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var userId = GetUserId(httpContextAccessor.HttpContext);
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            return await notificationService.DeleteNotificationAsync(id, userId, companyId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("DELETE_NOTIFICATION_ERROR", $"Failed to delete notification: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<NotificationPreferenceDto> CreateNotificationPreference(
        CreateNotificationPreferenceDto input,
        [Service] INotificationService notificationService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateNotificationPreferenceDto> validator,
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
            return await notificationService.CreateNotificationPreferenceAsync(input, userId, companyId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("CREATE_NOTIFICATION_PREFERENCE_ERROR", $"Failed to create notification preference: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<NotificationPreferenceDto?> UpdateNotificationPreference(
        int id,
        UpdateNotificationPreferenceDto input,
        [Service] INotificationService notificationService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateNotificationPreferenceDto> validator,
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
            return await notificationService.UpdateNotificationPreferenceAsync(id, input, userId, companyId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("UPDATE_NOTIFICATION_PREFERENCE_ERROR", $"Failed to update notification preference: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<bool> DeleteNotificationPreference(
        int id,
        [Service] INotificationService notificationService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var userId = GetUserId(httpContextAccessor.HttpContext);
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            return await notificationService.DeleteNotificationPreferenceAsync(id, userId, companyId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("DELETE_NOTIFICATION_PREFERENCE_ERROR", $"Failed to delete notification preference: {ex.Message}"));
        }
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Subscription))]
public class NotificationSubscription : BaseResolver
{
    [Authorize]
    [Subscribe(With = nameof(OnNotificationCreatedAsync))]
    public NotificationDto OnNotificationCreated(
        [EventMessage] NotificationDto notification)
    {
        return notification;
    }

    public async ValueTask<ISourceStream<NotificationDto>> OnNotificationCreatedAsync(
        [Service] ITopicEventReceiver eventReceiver,
        [Service] IHttpContextAccessor httpContextAccessor,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContextAccessor.HttpContext);
        return await eventReceiver.SubscribeAsync<NotificationDto>($"notification_{userId}", cancellationToken);
    }

    [Authorize]
    [Subscribe(With = nameof(OnNotificationCountChangedAsync))]
    public NotificationCountDto OnNotificationCountChanged(
        [EventMessage] NotificationCountDto count)
    {
        return count;
    }

    public async ValueTask<ISourceStream<NotificationCountDto>> OnNotificationCountChangedAsync(
        [Service] ITopicEventReceiver eventReceiver,
        [Service] IHttpContextAccessor httpContextAccessor,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContextAccessor.HttpContext);
        return await eventReceiver.SubscribeAsync<NotificationCountDto>($"notification_count_{userId}", cancellationToken);
    }
}

