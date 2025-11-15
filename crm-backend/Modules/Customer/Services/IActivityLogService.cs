using crm_backend.Modules.Customer.DTOs;

namespace crm_backend.Modules.Customer.Services;

public interface IActivityLogService
{
    Task<ActivityLogDto> CreateActivityLogAsync(CreateActivityLogDto dto, int userId, string? ipAddress = null, string? userAgent = null);
    Task<IEnumerable<ActivityLogDto>> GetCustomerActivitiesAsync(int customerId, int userId, int page = 1, int pageSize = 50);
    Task<IEnumerable<ActivityLogSummaryDto>> GetCustomerActivitySummaryAsync(int customerId, int userId);
    Task<IEnumerable<ActivityLogDto>> GetRecentActivitiesAsync(int companyId, int limit = 20);
}