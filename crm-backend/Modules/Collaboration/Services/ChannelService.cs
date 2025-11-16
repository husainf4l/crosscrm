using System.Text.Json;
using crm_backend.Data;
using crm_backend.Modules.Collaboration.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Collaboration.Services;

public class ChannelService : IChannelService
{
    private readonly CrmDbContext _context;

    public ChannelService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ChannelDto>> GetChannelsByCompanyAsync(int companyId, int? userId = null)
    {
        var query = _context.Channels
            .Include(c => c.Team)
            .Include(c => c.Customer)
            .Include(c => c.CreatedByUser)
            .Where(c => c.CompanyId == companyId && !c.IsArchived)
            .AsQueryable();

        // Filter by user membership if userId provided
        if (userId.HasValue)
        {
            var userChannelIds = await _context.ChannelMembers
                .Where(cm => cm.UserId == userId.Value)
                .Select(cm => cm.ChannelId)
                .ToListAsync();

            query = query.Where(c => userChannelIds.Contains(c.Id) || c.Type == ChannelType.Public);
        }

        var channels = await query
            .Select(c => new ChannelDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Type = c.Type.ToString(),
                IsArchived = c.IsArchived,
                TeamId = c.TeamId,
                TeamName = c.Team != null ? c.Team.Name : null,
                CustomerId = c.CustomerId,
                CustomerName = c.Customer != null ? c.Customer.Name : null,
                CreatedByUserId = c.CreatedByUserId,
                CreatedByUserName = c.CreatedByUser.Name,
                CompanyId = c.CompanyId,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                MemberCount = _context.ChannelMembers.Count(cm => cm.ChannelId == c.Id),
                LastMessageAt = _context.Messages
                    .Where(m => m.ChannelId == c.Id)
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => (DateTime?)m.CreatedAt)
                    .FirstOrDefault()
            })
            .ToListAsync();

        // Add unread count if userId provided
        if (userId.HasValue)
        {
            foreach (var channel in channels)
            {
                channel.UnreadCount = await GetUnreadCountAsync(channel.Id, userId.Value, companyId);
            }
        }

        return channels;
    }

    public async Task<IEnumerable<ChannelDto>> GetChannelsByTeamAsync(int teamId, int companyId)
    {
        var channels = await _context.Channels
            .Include(c => c.Team)
            .Include(c => c.Customer)
            .Include(c => c.CreatedByUser)
            .Where(c => c.TeamId == teamId && c.CompanyId == companyId && !c.IsArchived)
            .Select(c => new ChannelDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Type = c.Type.ToString(),
                IsArchived = c.IsArchived,
                TeamId = c.TeamId,
                TeamName = c.Team != null ? c.Team.Name : null,
                CustomerId = c.CustomerId,
                CustomerName = c.Customer != null ? c.Customer.Name : null,
                CreatedByUserId = c.CreatedByUserId,
                CreatedByUserName = c.CreatedByUser.Name,
                CompanyId = c.CompanyId,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                MemberCount = _context.ChannelMembers.Count(cm => cm.ChannelId == c.Id),
                LastMessageAt = _context.Messages
                    .Where(m => m.ChannelId == c.Id)
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => (DateTime?)m.CreatedAt)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return channels;
    }

    public async Task<IEnumerable<ChannelDto>> GetChannelsByCustomerAsync(int customerId, int companyId)
    {
        var channels = await _context.Channels
            .Include(c => c.Team)
            .Include(c => c.Customer)
            .Include(c => c.CreatedByUser)
            .Where(c => c.CustomerId == customerId && c.CompanyId == companyId && !c.IsArchived)
            .Select(c => new ChannelDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Type = c.Type.ToString(),
                IsArchived = c.IsArchived,
                TeamId = c.TeamId,
                TeamName = c.Team != null ? c.Team.Name : null,
                CustomerId = c.CustomerId,
                CustomerName = c.Customer != null ? c.Customer.Name : null,
                CreatedByUserId = c.CreatedByUserId,
                CreatedByUserName = c.CreatedByUser.Name,
                CompanyId = c.CompanyId,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                MemberCount = _context.ChannelMembers.Count(cm => cm.ChannelId == c.Id),
                LastMessageAt = _context.Messages
                    .Where(m => m.ChannelId == c.Id)
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => (DateTime?)m.CreatedAt)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return channels;
    }

    public async Task<ChannelDto?> GetChannelByIdAsync(int id, int? userId = null)
    {
        var channel = await _context.Channels
            .Include(c => c.Team)
            .Include(c => c.Customer)
            .Include(c => c.CreatedByUser)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (channel == null) return null;

        var memberCount = await _context.ChannelMembers.CountAsync(cm => cm.ChannelId == channel.Id);
        var lastMessage = await _context.Messages
            .Where(m => m.ChannelId == channel.Id)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync();

        var dto = new ChannelDto
        {
            Id = channel.Id,
            Name = channel.Name,
            Description = channel.Description,
            Type = channel.Type.ToString(),
            IsArchived = channel.IsArchived,
            TeamId = channel.TeamId,
            TeamName = channel.Team?.Name,
            CustomerId = channel.CustomerId,
            CustomerName = channel.Customer?.Name,
            CreatedByUserId = channel.CreatedByUserId,
            CreatedByUserName = channel.CreatedByUser.Name,
            CompanyId = channel.CompanyId,
            CreatedAt = channel.CreatedAt,
            UpdatedAt = channel.UpdatedAt,
            MemberCount = memberCount,
            LastMessageAt = lastMessage?.CreatedAt
        };

        if (userId.HasValue)
        {
            dto.UnreadCount = await GetUnreadCountAsync(channel.Id, userId.Value, channel.CompanyId);
        }

        return dto;
    }

    public async Task<ChannelDto> CreateChannelAsync(CreateChannelDto dto, int companyId, int createdByUserId)
    {
        // Parse channel type
        if (!Enum.TryParse<ChannelType>(dto.Type, true, out var channelType))
        {
            throw new ArgumentException($"Invalid channel type: {dto.Type}");
        }

        // Verify team exists (if provided)
        if (dto.TeamId.HasValue)
        {
            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.Id == dto.TeamId.Value && t.CompanyId == companyId);
            if (team == null)
            {
                throw new InvalidOperationException("Team not found or does not belong to the company.");
            }
        }

        // Verify customer exists (if provided)
        if (dto.CustomerId.HasValue)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == dto.CustomerId.Value && c.CompanyId == companyId);
            if (customer == null)
            {
                throw new InvalidOperationException("Customer not found or does not belong to the company.");
            }
        }

        var channel = new Channel
        {
            Name = dto.Name,
            Description = dto.Description,
            Type = channelType,
            TeamId = dto.TeamId,
            CustomerId = dto.CustomerId,
            CreatedByUserId = createdByUserId,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Channels.Add(channel);
        await _context.SaveChangesAsync();

        // Add creator as member
        var creatorMember = new ChannelMember
        {
            ChannelId = channel.Id,
            UserId = createdByUserId,
            Role = ChannelMemberRole.Admin,
            JoinedAt = DateTime.UtcNow
        };
        _context.ChannelMembers.Add(creatorMember);

        // Add other members if provided
        if (dto.MemberUserIds != null && dto.MemberUserIds.Count > 0)
        {
            foreach (var memberUserId in dto.MemberUserIds)
            {
                if (memberUserId == createdByUserId) continue; // Skip creator

                var member = new ChannelMember
                {
                    ChannelId = channel.Id,
                    UserId = memberUserId,
                    Role = ChannelMemberRole.Member,
                    JoinedAt = DateTime.UtcNow
                };
                _context.ChannelMembers.Add(member);
            }
        }

        await _context.SaveChangesAsync();

        return await GetChannelByIdAsync(channel.Id)
            ?? throw new InvalidOperationException("Failed to retrieve created channel");
    }

    public async Task<ChannelDto?> UpdateChannelAsync(int id, UpdateChannelDto dto, int companyId)
    {
        var channel = await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId);
        if (channel == null) return null;

        if (!string.IsNullOrEmpty(dto.Name))
            channel.Name = dto.Name;

        if (dto.Description != null)
            channel.Description = dto.Description;

        if (dto.IsArchived.HasValue)
            channel.IsArchived = dto.IsArchived.Value;

        channel.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetChannelByIdAsync(channel.Id);
    }

    public async Task<bool> DeleteChannelAsync(int id, int companyId)
    {
        var channel = await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId);
        if (channel == null) return false;

        _context.Channels.Remove(channel);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ChannelMemberDto> AddChannelMemberAsync(AddChannelMemberDto dto, int companyId)
    {
        // Verify channel exists and belongs to company
        var channel = await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == dto.ChannelId && c.CompanyId == companyId);
        if (channel == null)
        {
            throw new InvalidOperationException("Channel not found or does not belong to the company.");
        }

        // Check if member already exists
        var existing = await _context.ChannelMembers
            .FirstOrDefaultAsync(cm => cm.ChannelId == dto.ChannelId && cm.UserId == dto.UserId);
        if (existing != null)
        {
            throw new InvalidOperationException("User is already a member of this channel.");
        }

        var role = ChannelMemberRole.Member;
        if (!string.IsNullOrEmpty(dto.Role) && Enum.TryParse<ChannelMemberRole>(dto.Role, true, out var parsedRole))
        {
            role = parsedRole;
        }

        var member = new ChannelMember
        {
            ChannelId = dto.ChannelId,
            UserId = dto.UserId,
            Role = role,
            JoinedAt = DateTime.UtcNow
        };

        _context.ChannelMembers.Add(member);
        await _context.SaveChangesAsync();

        return await GetChannelMemberByIdAsync(member.Id)
            ?? throw new InvalidOperationException("Failed to retrieve created member");
    }

    public async Task<bool> RemoveChannelMemberAsync(int channelId, int userId, int companyId)
    {
        var member = await _context.ChannelMembers
            .Include(cm => cm.Channel)
            .FirstOrDefaultAsync(cm => cm.ChannelId == channelId && cm.UserId == userId);

        if (member == null || member.Channel.CompanyId != companyId) return false;

        _context.ChannelMembers.Remove(member);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<ChannelMemberDto>> GetChannelMembersAsync(int channelId, int companyId)
    {
        var members = await _context.ChannelMembers
            .Include(cm => cm.Channel)
            .Include(cm => cm.User)
            .Where(cm => cm.ChannelId == channelId && cm.Channel.CompanyId == companyId)
            .Select(cm => new ChannelMemberDto
            {
                Id = cm.Id,
                ChannelId = cm.ChannelId,
                UserId = cm.UserId,
                UserName = cm.User.Name,
                UserEmail = cm.User.Email,
                Role = cm.Role.ToString(),
                JoinedAt = cm.JoinedAt,
                LastReadAt = cm.LastReadAt,
                NotificationSettings = !string.IsNullOrEmpty(cm.NotificationSettings)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(cm.NotificationSettings)
                    : null
            })
            .ToListAsync();

        return members;
    }

    public async Task<int> GetUnreadCountAsync(int channelId, int userId, int companyId)
    {
        var member = await _context.ChannelMembers
            .FirstOrDefaultAsync(cm => cm.ChannelId == channelId && cm.UserId == userId);

        if (member == null) return 0;

        var lastReadAt = member.LastReadAt ?? member.JoinedAt;

        var unreadCount = await _context.Messages
            .CountAsync(m => m.ChannelId == channelId
                && m.CompanyId == companyId
                && m.CreatedAt > lastReadAt
                && !m.IsDeleted);

        return unreadCount;
    }

    public async Task<bool> MarkChannelAsReadAsync(int channelId, int userId, int companyId)
    {
        var member = await _context.ChannelMembers
            .Include(cm => cm.Channel)
            .FirstOrDefaultAsync(cm => cm.ChannelId == channelId && cm.UserId == userId);

        if (member == null || member.Channel.CompanyId != companyId) return false;

        member.LastReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<ChannelMemberDto?> GetChannelMemberByIdAsync(int id)
    {
        var member = await _context.ChannelMembers
            .Include(cm => cm.User)
            .FirstOrDefaultAsync(cm => cm.Id == id);

        if (member == null) return null;

        return new ChannelMemberDto
        {
            Id = member.Id,
            ChannelId = member.ChannelId,
            UserId = member.UserId,
            UserName = member.User.Name,
            UserEmail = member.User.Email,
            Role = member.Role.ToString(),
            JoinedAt = member.JoinedAt,
            LastReadAt = member.LastReadAt,
            NotificationSettings = !string.IsNullOrEmpty(member.NotificationSettings)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(member.NotificationSettings)
                : null
        };
    }
}

