using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.Services;

public interface INotificationService
{
    Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto, int companyId);
    Task<IEnumerable<NotificationDto>> GetNotificationsForUserAsync(int userId, int companyId, bool? unreadOnly = null, int? limit = null);
    Task<NotificationDto?> GetNotificationByIdAsync(int id, int userId, int companyId);
    Task<bool> MarkNotificationAsReadAsync(int id, int userId, int companyId);
    Task<int> MarkAllNotificationsAsReadAsync(int userId, int companyId);
    Task<NotificationCountDto> GetNotificationCountAsync(int userId, int companyId);
    Task<bool> DeleteNotificationAsync(int id, int userId, int companyId);

    // Notification Preferences
    Task<IEnumerable<NotificationPreferenceDto>> GetNotificationPreferencesAsync(int userId, int companyId);
    Task<NotificationPreferenceDto> CreateNotificationPreferenceAsync(CreateNotificationPreferenceDto dto, int userId, int companyId);
    Task<NotificationPreferenceDto?> UpdateNotificationPreferenceAsync(int id, UpdateNotificationPreferenceDto dto, int userId, int companyId);
    Task<bool> DeleteNotificationPreferenceAsync(int id, int userId, int companyId);

    // Helper methods for creating notifications from events
    Task NotifyUserMentionedAsync(int userId, int mentionedByUserId, int messageId, int companyId);
    Task NotifyNewMessageAsync(int userId, int channelId, int messageId, int companyId);
    Task NotifyActivityAsync(int userId, string activityType, int entityId, string entityType, int companyId);
}

