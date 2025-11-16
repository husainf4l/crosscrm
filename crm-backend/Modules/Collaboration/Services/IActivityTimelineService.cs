using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.Services;

public interface IActivityTimelineService
{
    Task<ActivityTimelineDto> LogActivityAsync(CreateActivityTimelineDto dto, int companyId, int? performedByUserId = null);
    Task<IEnumerable<ActivityTimelineDto>> GetTimelineByEntityAsync(string entityType, int entityId, int companyId, int? userId = null);
    Task<IEnumerable<ActivityTimelineDto>> GetUserActivityFeedAsync(int userId, int companyId, int skip = 0, int take = 50);
    Task<IEnumerable<ActivityTimelineDto>> GetActivitiesByTypeAsync(string activityType, int companyId, int? userId = null, int skip = 0, int take = 50);
    Task<bool> MarkActivityAsReadAsync(int activityId, int userId, int companyId);
    Task<int> GetUnreadActivityCountAsync(int userId, int companyId);
}

