using System.Text.Json;
using crm_backend.Data;
using crm_backend.Modules.Collaboration.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Collaboration.Services;

public class NotificationService : INotificationService
{
    private readonly CrmDbContext _context;

    public NotificationService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto, int companyId)
    {
        // Verify user exists and belongs to company
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == dto.UserId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        // Parse enums
        if (!Enum.TryParse<NotificationType>(dto.Type, true, out var notificationType))
        {
            throw new ArgumentException($"Invalid notification type: {dto.Type}");
        }

        if (!Enum.TryParse<NotificationPriority>(dto.Priority, true, out var priority))
        {
            priority = NotificationPriority.Medium;
        }

        var notification = new Notification
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Message = dto.Message,
            Type = notificationType,
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            ActionUrl = dto.ActionUrl,
            Priority = priority,
            Metadata = dto.Metadata != null ? JsonSerializer.Serialize(dto.Metadata) : null,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return await GetNotificationByIdAsync(notification.Id, dto.UserId, companyId)
            ?? throw new InvalidOperationException("Failed to retrieve created notification");
    }

    public async Task<IEnumerable<NotificationDto>> GetNotificationsForUserAsync(int userId, int companyId, bool? unreadOnly = null, int? limit = null)
    {
        var query = _context.Notifications
            .Include(n => n.User)
            .Where(n => n.UserId == userId && n.CompanyId == companyId);

        if (unreadOnly.HasValue && unreadOnly.Value)
        {
            query = query.Where(n => !n.IsRead);
        }

        query = query.OrderByDescending(n => n.CreatedAt);

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        var notifications = await query.ToListAsync();

        return notifications.Select(n => MapToDto(n));
    }

    public async Task<NotificationDto?> GetNotificationByIdAsync(int id, int userId, int companyId)
    {
        var notification = await _context.Notifications
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId && n.CompanyId == companyId);

        if (notification == null) return null;

        return MapToDto(notification);
    }

    public async Task<bool> MarkNotificationAsReadAsync(int id, int userId, int companyId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId && n.CompanyId == companyId);

        if (notification == null) return false;

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<int> MarkAllNotificationsAsReadAsync(int userId, int companyId)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && n.CompanyId == companyId && !n.IsRead)
            .ToListAsync();

        var count = unreadNotifications.Count;

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return count;
    }

    public async Task<NotificationCountDto> GetNotificationCountAsync(int userId, int companyId)
    {
        var totalCount = await _context.Notifications
            .CountAsync(n => n.UserId == userId && n.CompanyId == companyId);

        var unreadCount = await _context.Notifications
            .CountAsync(n => n.UserId == userId && n.CompanyId == companyId && !n.IsRead);

        return new NotificationCountDto
        {
            TotalCount = totalCount,
            UnreadCount = unreadCount
        };
    }

    public async Task<bool> DeleteNotificationAsync(int id, int userId, int companyId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId && n.CompanyId == companyId);

        if (notification == null) return false;

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<NotificationPreferenceDto>> GetNotificationPreferencesAsync(int userId, int companyId)
    {
        var preferences = await _context.NotificationPreferences
            .Include(np => np.User)
            .Where(np => np.UserId == userId && np.CompanyId == companyId)
            .Select(np => new NotificationPreferenceDto
            {
                Id = np.Id,
                UserId = np.UserId,
                UserName = np.User.Name,
                NotificationType = np.NotificationType.ToString(),
                Channel = np.Channel.ToString(),
                IsEnabled = np.IsEnabled,
                CompanyId = np.CompanyId,
                CreatedAt = np.CreatedAt,
                UpdatedAt = np.UpdatedAt
            })
            .ToListAsync();

        return preferences;
    }

    public async Task<NotificationPreferenceDto> CreateNotificationPreferenceAsync(CreateNotificationPreferenceDto dto, int userId, int companyId)
    {
        // Verify user exists
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        // Parse enums
        if (!Enum.TryParse<NotificationType>(dto.NotificationType, true, out var notificationType))
        {
            throw new ArgumentException($"Invalid notification type: {dto.NotificationType}");
        }

        if (!Enum.TryParse<NotificationChannel>(dto.Channel, true, out var channel))
        {
            throw new ArgumentException($"Invalid channel: {dto.Channel}");
        }

        // Check if preference already exists
        var existing = await _context.NotificationPreferences
            .FirstOrDefaultAsync(np => np.UserId == userId
                && np.NotificationType == notificationType
                && np.Channel == channel
                && np.CompanyId == companyId);

        if (existing != null)
        {
            // Update existing preference
            existing.IsEnabled = dto.IsEnabled;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new NotificationPreferenceDto
            {
                Id = existing.Id,
                UserId = existing.UserId,
                UserName = user.Name,
                NotificationType = existing.NotificationType.ToString(),
                Channel = existing.Channel.ToString(),
                IsEnabled = existing.IsEnabled,
                CompanyId = existing.CompanyId,
                CreatedAt = existing.CreatedAt,
                UpdatedAt = existing.UpdatedAt
            };
        }

        var preference = new NotificationPreference
        {
            UserId = userId,
            NotificationType = notificationType,
            Channel = channel,
            IsEnabled = dto.IsEnabled,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.NotificationPreferences.Add(preference);
        await _context.SaveChangesAsync();

        return new NotificationPreferenceDto
        {
            Id = preference.Id,
            UserId = preference.UserId,
            UserName = user.Name,
            NotificationType = preference.NotificationType.ToString(),
            Channel = preference.Channel.ToString(),
            IsEnabled = preference.IsEnabled,
            CompanyId = preference.CompanyId,
            CreatedAt = preference.CreatedAt,
            UpdatedAt = preference.UpdatedAt
        };
    }

    public async Task<NotificationPreferenceDto?> UpdateNotificationPreferenceAsync(int id, UpdateNotificationPreferenceDto dto, int userId, int companyId)
    {
        var preference = await _context.NotificationPreferences
            .Include(np => np.User)
            .FirstOrDefaultAsync(np => np.Id == id && np.UserId == userId && np.CompanyId == companyId);

        if (preference == null) return null;

        if (dto.IsEnabled.HasValue)
        {
            preference.IsEnabled = dto.IsEnabled.Value;
        }

        preference.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new NotificationPreferenceDto
        {
            Id = preference.Id,
            UserId = preference.UserId,
            UserName = preference.User.Name,
            NotificationType = preference.NotificationType.ToString(),
            Channel = preference.Channel.ToString(),
            IsEnabled = preference.IsEnabled,
            CompanyId = preference.CompanyId,
            CreatedAt = preference.CreatedAt,
            UpdatedAt = preference.UpdatedAt
        };
    }

    public async Task<bool> DeleteNotificationPreferenceAsync(int id, int userId, int companyId)
    {
        var preference = await _context.NotificationPreferences
            .FirstOrDefaultAsync(np => np.Id == id && np.UserId == userId && np.CompanyId == companyId);

        if (preference == null) return false;

        _context.NotificationPreferences.Remove(preference);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task NotifyUserMentionedAsync(int userId, int mentionedByUserId, int messageId, int companyId)
    {
        var mentionedByUser = await _context.Users.FindAsync(mentionedByUserId);
        if (mentionedByUser == null) return;

        var dto = new CreateNotificationDto
        {
            UserId = userId,
            Title = "You were mentioned",
            Message = $"{mentionedByUser.Name} mentioned you in a message",
            Type = NotificationType.Mention.ToString(),
            EntityType = "Message",
            EntityId = messageId,
            ActionUrl = $"/messages/{messageId}",
            Priority = NotificationPriority.High.ToString()
        };

        await CreateNotificationAsync(dto, companyId);
    }

    public async Task NotifyNewMessageAsync(int userId, int channelId, int messageId, int companyId)
    {
        var channel = await _context.Channels.FindAsync(channelId);
        if (channel == null) return;

        var dto = new CreateNotificationDto
        {
            UserId = userId,
            Title = $"New message in {channel.Name}",
            Message = "You have a new message",
            Type = NotificationType.Message.ToString(),
            EntityType = "Message",
            EntityId = messageId,
            ActionUrl = $"/channels/{channelId}/messages/{messageId}",
            Priority = NotificationPriority.Medium.ToString()
        };

        await CreateNotificationAsync(dto, companyId);
    }

    public async Task NotifyActivityAsync(int userId, string activityType, int entityId, string entityType, int companyId)
    {
        var dto = new CreateNotificationDto
        {
            UserId = userId,
            Title = "New activity",
            Message = $"Activity: {activityType}",
            Type = NotificationType.Activity.ToString(),
            EntityType = entityType,
            EntityId = entityId,
            ActionUrl = $"/{entityType.ToLower()}/{entityId}",
            Priority = NotificationPriority.Low.ToString()
        };

        await CreateNotificationAsync(dto, companyId);
    }

    private NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            UserName = notification.User.Name,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type.ToString(),
            EntityType = notification.EntityType,
            EntityId = notification.EntityId,
            ActionUrl = notification.ActionUrl,
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            Priority = notification.Priority.ToString(),
            Metadata = !string.IsNullOrEmpty(notification.Metadata)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(notification.Metadata)
                : null,
            CompanyId = notification.CompanyId,
            CreatedAt = notification.CreatedAt
        };
    }
}

