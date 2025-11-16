using crm_backend.Modules.Customer.DTOs;
using crm_backend.Modules.Customer.Services;

namespace crm_backend.Modules.Customer;

[ExtendObjectType("Mutation")]
public class ActivityLogMutation
{
    public async Task<ActivityLogDto> CreateActivityLog(
        CreateActivityLogDto input,
        [Service] IActivityLogService activityLogService,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = GetUserIdFromContext(httpContextAccessor);
        var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

        return await activityLogService.CreateActivityLogAsync(input, userId, ipAddress, userAgent);
    }

    [ExtendObjectType("Query")]
    public class ActivityLogQuery
    {
        public async Task<IEnumerable<ActivityLogDto>> GetCustomerActivities(
            [Service] IActivityLogService activityLogService,
            [Service] IHttpContextAccessor httpContextAccessor,
            int customerId,
            int page = 1,
            int pageSize = 50)
        {
            var userId = GetUserIdFromContext(httpContextAccessor);
            return await activityLogService.GetCustomerActivitiesAsync(customerId, userId, page, pageSize);
        }

        public async Task<IEnumerable<ActivityLogSummaryDto>> GetCustomerActivitySummary(
            [Service] IActivityLogService activityLogService,
            [Service] IHttpContextAccessor httpContextAccessor,
            int customerId)
        {
            var userId = GetUserIdFromContext(httpContextAccessor);
            return await activityLogService.GetCustomerActivitySummaryAsync(customerId, userId);
        }

        public async Task<IEnumerable<ActivityLogDto>> GetRecentActivities(
            [Service] IActivityLogService activityLogService,
            [Service] IHttpContextAccessor httpContextAccessor,
            int limit = 20)
        {
            var userId = GetUserIdFromContext(httpContextAccessor);
            var companyId = await GetCompanyIdFromUser(httpContextAccessor, userId);
            return await activityLogService.GetRecentActivitiesAsync(companyId, limit);
        }
    }

    private static int GetUserIdFromContext(IHttpContextAccessor httpContextAccessor)
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            throw new GraphQLException("Unauthorized");
        return userId;
    }

    private static async Task<int> GetCompanyIdFromUser(IHttpContextAccessor httpContextAccessor, int userId)
    {
        // This would need to be injected as a service to get the company ID
        // For now, we'll assume it's available through the user context
        var companyIdClaim = httpContextAccessor.HttpContext?.User.FindFirst("companyId")?.Value;
        if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out var companyId))
            throw new GraphQLException("Company context not found");
        return companyId;
    }
}
