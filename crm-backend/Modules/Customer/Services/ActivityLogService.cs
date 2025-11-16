using crm_backend.Data;
using crm_backend.Modules.Customer.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Customer.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly CrmDbContext _context;

    public ActivityLogService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<ActivityLogDto> CreateActivityLogAsync(CreateActivityLogDto dto, int userId, string? ipAddress = null, string? userAgent = null)
    {
        // Verify customer exists and user has access
        var customer = await _context.Customers
            .Include(c => c.Company)
            .FirstOrDefaultAsync(c => c.Id == dto.CustomerId);

        if (customer == null)
            throw new Exception("Customer not found");

        // Verify user has access to this customer's company
        var userCompany = await _context.UserCompanies
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompanyId == customer.CompanyId && uc.IsActive);

        if (userCompany == null)
            throw new Exception("Access denied");

        var activityLog = new ActivityLog
        {
            CustomerId = dto.CustomerId,
            ActivityType = dto.ActivityType,
            Description = dto.Description,
            ReferenceType = dto.ReferenceType,
            ReferenceId = dto.ReferenceId,
            UserId = userId,
            Details = dto.Details,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        _context.ActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync();

        return await GetActivityLogDtoAsync(activityLog.Id);
    }

    public async Task<IEnumerable<ActivityLogDto>> GetCustomerActivitiesAsync(int customerId, int userId, int page = 1, int pageSize = 50)
    {
        // Verify access
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null)
            throw new Exception("Customer not found");

        var userCompany = await _context.UserCompanies
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompanyId == customer.CompanyId && uc.IsActive);

        if (userCompany == null)
            throw new Exception("Access denied");

        var skip = (page - 1) * pageSize;

        var activities = await _context.ActivityLogs
            .Where(a => a.CustomerId == customerId)
            .Include(a => a.User)
            .Include(a => a.Customer)
            .OrderByDescending(a => a.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return activities.Select(a => new ActivityLogDto
        {
            Id = a.Id,
            CustomerId = a.CustomerId,
            CustomerName = a.Customer.Name,
            ActivityType = a.ActivityType,
            Description = a.Description,
            ReferenceType = a.ReferenceType,
            ReferenceId = a.ReferenceId,
            UserId = a.UserId,
            UserName = a.User.Name,
            Details = a.Details,
            CreatedAt = a.CreatedAt,
            IpAddress = a.IpAddress
        });
    }

    public async Task<IEnumerable<ActivityLogSummaryDto>> GetCustomerActivitySummaryAsync(int customerId, int userId)
    {
        // Verify access (same as above)
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null)
            throw new Exception("Customer not found");

        var userCompany = await _context.UserCompanies
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompanyId == customer.CompanyId && uc.IsActive);

        if (userCompany == null)
            throw new Exception("Access denied");

        var summary = await _context.ActivityLogs
            .Where(a => a.CustomerId == customerId)
            .GroupBy(a => a.ActivityType)
            .Select(g => new ActivityLogSummaryDto
            {
                ActivityType = g.Key,
                Count = g.Count(),
                LastActivity = g.Max(a => a.CreatedAt)
            })
            .ToListAsync();

        return summary;
    }

    public async Task<IEnumerable<ActivityLogDto>> GetRecentActivitiesAsync(int companyId, int limit = 20)
    {
        var activities = await _context.ActivityLogs
            .Where(a => a.Customer.CompanyId == companyId)
            .Include(a => a.User)
            .Include(a => a.Customer)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return activities.Select(a => new ActivityLogDto
        {
            Id = a.Id,
            CustomerId = a.CustomerId,
            CustomerName = a.Customer.Name,
            ActivityType = a.ActivityType,
            Description = a.Description,
            ReferenceType = a.ReferenceType,
            ReferenceId = a.ReferenceId,
            UserId = a.UserId,
            UserName = a.User.Name,
            Details = a.Details,
            CreatedAt = a.CreatedAt,
            IpAddress = a.IpAddress
        });
    }

    private async Task<ActivityLogDto> GetActivityLogDtoAsync(int id)
    {
        var activity = await _context.ActivityLogs
            .Include(a => a.User)
            .Include(a => a.Customer)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (activity == null)
            throw new Exception("Activity log not found");

        return new ActivityLogDto
        {
            Id = activity.Id,
            CustomerId = activity.CustomerId,
            CustomerName = activity.Customer.Name,
            ActivityType = activity.ActivityType,
            Description = activity.Description,
            ReferenceType = activity.ReferenceType,
            ReferenceId = activity.ReferenceId,
            UserId = activity.UserId,
            UserName = activity.User.Name,
            Details = activity.Details,
            CreatedAt = activity.CreatedAt,
            IpAddress = activity.IpAddress
        };
    }
}
