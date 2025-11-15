using System.Text.Json;
using crm_backend.Data;
using crm_backend.Modules.Collaboration;
using crm_backend.Modules.Collaboration.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Collaboration.Services;

public class ActivityTimelineService : IActivityTimelineService
{
    private readonly CrmDbContext _context;

    public ActivityTimelineService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<ActivityTimelineDto> LogActivityAsync(CreateActivityTimelineDto dto, int companyId, int? performedByUserId = null)
    {
        // Parse activity type
        if (!Enum.TryParse<ActivityType>(dto.Type, true, out var activityType))
        {
            throw new ArgumentException($"Invalid activity type: {dto.Type}");
        }

        var activity = new ActivityTimeline
        {
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            Type = activityType,
            Title = dto.Title,
            Description = dto.Description,
            PerformedByUserId = performedByUserId,
            AIAgentId = dto.AIAgentId,
            Metadata = dto.Metadata != null ? JsonSerializer.Serialize(dto.Metadata) : null,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.ActivityTimelines.Add(activity);
        await _context.SaveChangesAsync();

        // Create activity feeds for notified users
        if (dto.NotifyUserIds != null && dto.NotifyUserIds.Count > 0)
        {
            foreach (var userId in dto.NotifyUserIds)
            {
                // Verify user belongs to company
                var userCompany = await _context.UserCompanies
                    .AnyAsync(uc => uc.UserId == userId && uc.CompanyId == companyId);
                if (!userCompany) continue;

                var feed = new ActivityFeed
                {
                    UserId = userId,
                    ActivityId = activity.Id,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ActivityFeeds.Add(feed);
            }
            await _context.SaveChangesAsync();
        }

        return await GetActivityByIdAsync(activity.Id) 
            ?? throw new InvalidOperationException("Failed to retrieve created activity");
    }

    public async Task<IEnumerable<ActivityTimelineDto>> GetTimelineByEntityAsync(string entityType, int entityId, int companyId, int? userId = null)
    {
        var activities = await _context.ActivityTimelines
            .Include(at => at.PerformedByUser)
            .Include(at => at.AIAgent)
            .Where(at => at.EntityType == entityType && at.EntityId == entityId && at.CompanyId == companyId)
            .OrderByDescending(at => at.CreatedAt)
            .Select(at => new ActivityTimelineDto
            {
                Id = at.Id,
                EntityType = at.EntityType,
                EntityId = at.EntityId,
                Type = at.Type.ToString(),
                Title = at.Title,
                Description = at.Description,
                PerformedByUserId = at.PerformedByUserId,
                PerformedByUserName = at.PerformedByUser != null ? at.PerformedByUser.Name : null,
                AIAgentId = at.AIAgentId,
                AIAgentName = at.AIAgent != null ? at.AIAgent.Name : null,
                Metadata = !string.IsNullOrEmpty(at.Metadata)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(at.Metadata)
                    : null,
                CompanyId = at.CompanyId,
                CreatedAt = at.CreatedAt,
                IsRead = userId.HasValue
                    ? _context.ActivityFeeds
                        .Any(af => af.ActivityId == at.Id && af.UserId == userId.Value && af.IsRead)
                    : false
            })
            .ToListAsync();

        return activities;
    }

    public async Task<IEnumerable<ActivityTimelineDto>> GetUserActivityFeedAsync(int userId, int companyId, int skip = 0, int take = 50)
    {
        var activities = await _context.ActivityFeeds
            .Include(af => af.Activity)
                .ThenInclude(a => a.PerformedByUser)
            .Include(af => af.Activity)
                .ThenInclude(a => a.AIAgent)
            .Where(af => af.UserId == userId && af.Activity.CompanyId == companyId)
            .OrderByDescending(af => af.Activity.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(af => new ActivityTimelineDto
            {
                Id = af.Activity.Id,
                EntityType = af.Activity.EntityType,
                EntityId = af.Activity.EntityId,
                Type = af.Activity.Type.ToString(),
                Title = af.Activity.Title,
                Description = af.Activity.Description,
                PerformedByUserId = af.Activity.PerformedByUserId,
                PerformedByUserName = af.Activity.PerformedByUser != null ? af.Activity.PerformedByUser.Name : null,
                AIAgentId = af.Activity.AIAgentId,
                AIAgentName = af.Activity.AIAgent != null ? af.Activity.AIAgent.Name : null,
                Metadata = !string.IsNullOrEmpty(af.Activity.Metadata)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(af.Activity.Metadata)
                    : null,
                CompanyId = af.Activity.CompanyId,
                CreatedAt = af.Activity.CreatedAt,
                IsRead = af.IsRead
            })
            .ToListAsync();

        return activities;
    }

    public async Task<IEnumerable<ActivityTimelineDto>> GetActivitiesByTypeAsync(string activityType, int companyId, int? userId = null, int skip = 0, int take = 50)
    {
        if (!Enum.TryParse<ActivityType>(activityType, true, out var type))
        {
            throw new ArgumentException($"Invalid activity type: {activityType}");
        }

        var query = _context.ActivityTimelines
            .Include(at => at.PerformedByUser)
            .Include(at => at.AIAgent)
            .Where(at => at.Type == type && at.CompanyId == companyId)
            .AsQueryable();

        // Filter by user if provided
        if (userId.HasValue)
        {
            query = query.Where(at => at.ActivityFeeds.Any(af => af.UserId == userId.Value));
        }

        var activities = await query
            .OrderByDescending(at => at.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(at => new ActivityTimelineDto
            {
                Id = at.Id,
                EntityType = at.EntityType,
                EntityId = at.EntityId,
                Type = at.Type.ToString(),
                Title = at.Title,
                Description = at.Description,
                PerformedByUserId = at.PerformedByUserId,
                PerformedByUserName = at.PerformedByUser != null ? at.PerformedByUser.Name : null,
                AIAgentId = at.AIAgentId,
                AIAgentName = at.AIAgent != null ? at.AIAgent.Name : null,
                Metadata = !string.IsNullOrEmpty(at.Metadata)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(at.Metadata)
                    : null,
                CompanyId = at.CompanyId,
                CreatedAt = at.CreatedAt,
                IsRead = userId.HasValue
                    ? _context.ActivityFeeds
                        .Any(af => af.ActivityId == at.Id && af.UserId == userId.Value && af.IsRead)
                    : false
            })
            .ToListAsync();

        return activities;
    }

    public async Task<bool> MarkActivityAsReadAsync(int activityId, int userId, int companyId)
    {
        var feed = await _context.ActivityFeeds
            .Include(af => af.Activity)
            .FirstOrDefaultAsync(af => af.ActivityId == activityId && af.UserId == userId);
        
        if (feed == null || feed.Activity.CompanyId != companyId) return false;

        feed.IsRead = true;
        feed.ReadAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> GetUnreadActivityCountAsync(int userId, int companyId)
    {
        var unreadCount = await _context.ActivityFeeds
            .Include(af => af.Activity)
            .CountAsync(af => af.UserId == userId 
                && af.Activity.CompanyId == companyId 
                && !af.IsRead);

        return unreadCount;
    }

    private async Task<ActivityTimelineDto?> GetActivityByIdAsync(int id)
    {
        var activity = await _context.ActivityTimelines
            .Include(at => at.PerformedByUser)
            .Include(at => at.AIAgent)
            .FirstOrDefaultAsync(at => at.Id == id);

        if (activity == null) return null;

        return new ActivityTimelineDto
        {
            Id = activity.Id,
            EntityType = activity.EntityType,
            EntityId = activity.EntityId,
            Type = activity.Type.ToString(),
            Title = activity.Title,
            Description = activity.Description,
            PerformedByUserId = activity.PerformedByUserId,
            PerformedByUserName = activity.PerformedByUser?.Name,
            AIAgentId = activity.AIAgentId,
            AIAgentName = activity.AIAgent?.Name,
            Metadata = !string.IsNullOrEmpty(activity.Metadata)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(activity.Metadata)
                : null,
            CompanyId = activity.CompanyId,
            CreatedAt = activity.CreatedAt,
            IsRead = false
        };
    }
}

