using System.Text.Json;
using crm_backend.Data;
using crm_backend.Modules.Collaboration.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Collaboration.Services;

public class MessageService : IMessageService
{
    private readonly CrmDbContext _context;

    public MessageService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MessageDto>> GetMessagesByChannelAsync(int channelId, int companyId, int? userId = null, int skip = 0, int take = 50)
    {
        var messages = await _context.Messages
            .Include(m => m.Channel)
            .Include(m => m.CreatedByUser)
            .Include(m => m.AIAgent)
            .Where(m => m.ChannelId == channelId && m.CompanyId == companyId && m.ParentMessageId == null && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                Content = m.Content,
                ContentType = m.ContentType.ToString(),
                ChannelId = m.ChannelId,
                ChannelName = m.Channel.Name,
                CreatedByUserId = m.CreatedByUserId,
                CreatedByUserName = m.CreatedByUser != null ? m.CreatedByUser.Name : null,
                AIAgentId = m.AIAgentId,
                AIAgentName = m.AIAgent != null ? m.AIAgent.Name : null,
                ParentMessageId = m.ParentMessageId,
                IsEdited = m.IsEdited,
                IsDeleted = m.IsDeleted,
                EditedAt = m.EditedAt,
                DeletedAt = m.DeletedAt,
                Reactions = !string.IsNullOrEmpty(m.Reactions)
                    ? JsonSerializer.Deserialize<Dictionary<string, List<int>>>(m.Reactions)
                    : null,
                CompanyId = m.CompanyId,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
                ThreadReplyCount = _context.Messages.Count(t => t.ParentMessageId == m.Id && !t.IsDeleted),
                AttachmentCount = _context.MessageAttachments.Count(a => a.MessageId == m.Id)
            })
            .ToListAsync();

        // Load mentions and attachments for each message
        foreach (var message in messages)
        {
            message.Mentions = await GetMessageMentionsAsync(message.Id);
            message.Attachments = await GetMessageAttachmentsAsync(message.Id);
        }

        return messages.OrderBy(m => m.CreatedAt);
    }

    public async Task<MessageDto?> GetMessageByIdAsync(int id, int? userId = null)
    {
        var message = await _context.Messages
            .Include(m => m.Channel)
            .Include(m => m.CreatedByUser)
            .Include(m => m.AIAgent)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (message == null) return null;

        var threadReplyCount = await _context.Messages.CountAsync(m => m.ParentMessageId == message.Id && !m.IsDeleted);
        var attachmentCount = await _context.MessageAttachments.CountAsync(a => a.MessageId == message.Id);

        var dto = new MessageDto
        {
            Id = message.Id,
            Content = message.Content,
            ContentType = message.ContentType.ToString(),
            ChannelId = message.ChannelId,
            ChannelName = message.Channel.Name,
            CreatedByUserId = message.CreatedByUserId,
            CreatedByUserName = message.CreatedByUser != null ? message.CreatedByUser.Name : null,
            AIAgentId = message.AIAgentId,
            AIAgentName = message.AIAgent != null ? message.AIAgent.Name : null,
            ParentMessageId = message.ParentMessageId,
            IsEdited = message.IsEdited,
            IsDeleted = message.IsDeleted,
            EditedAt = message.EditedAt,
            DeletedAt = message.DeletedAt,
            Reactions = !string.IsNullOrEmpty(message.Reactions)
                ? JsonSerializer.Deserialize<Dictionary<string, List<int>>>(message.Reactions)
                : null,
            CompanyId = message.CompanyId,
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt,
            ThreadReplyCount = threadReplyCount,
            AttachmentCount = attachmentCount
        };

        dto.Mentions = await GetMessageMentionsAsync(message.Id);
        dto.Attachments = await GetMessageAttachmentsAsync(message.Id);

        return dto;
    }

    public async Task<MessageDto> SendMessageAsync(CreateMessageDto dto, int companyId, int? userId = null)
    {
        // Verify channel exists and user is member (if userId provided)
        var channel = await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == dto.ChannelId && c.CompanyId == companyId);
        if (channel == null)
        {
            throw new InvalidOperationException("Channel not found or does not belong to the company.");
        }

        if (userId.HasValue)
        {
            var isMember = await _context.ChannelMembers
                .AnyAsync(cm => cm.ChannelId == dto.ChannelId && cm.UserId == userId.Value);
            if (!isMember && channel.Type != ChannelType.Public)
            {
                throw new UnauthorizedAccessException("You are not a member of this channel.");
            }
        }

        // Parse content type
        if (!Enum.TryParse<MessageContentType>(dto.ContentType, true, out var contentType))
        {
            contentType = MessageContentType.Text;
        }

        var message = new Message
        {
            ChannelId = dto.ChannelId,
            Content = dto.Content,
            ContentType = contentType,
            CreatedByUserId = userId,
            AIAgentId = dto.AIAgentId,
            ParentMessageId = dto.ParentMessageId,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Handle mentions
        if (dto.MentionedUserIds != null && dto.MentionedUserIds.Count > 0)
        {
            foreach (var mentionedUserId in dto.MentionedUserIds)
            {
                var mention = new MessageMention
                {
                    MessageId = message.Id,
                    MentionedUserId = mentionedUserId,
                    CompanyId = companyId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.MessageMentions.Add(mention);
            }
        }

        if (dto.MentionedTeamIds != null && dto.MentionedTeamIds.Count > 0)
        {
            foreach (var mentionedTeamId in dto.MentionedTeamIds)
            {
                var mention = new MessageMention
                {
                    MessageId = message.Id,
                    MentionedTeamId = mentionedTeamId,
                    CompanyId = companyId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.MessageMentions.Add(mention);
            }
        }

        if (dto.MentionedAIAgentIds != null && dto.MentionedAIAgentIds.Count > 0)
        {
            foreach (var mentionedAIAgentId in dto.MentionedAIAgentIds)
            {
                var mention = new MessageMention
                {
                    MessageId = message.Id,
                    MentionedAIAgentId = mentionedAIAgentId,
                    CompanyId = companyId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.MessageMentions.Add(mention);
            }
        }

        await _context.SaveChangesAsync();

        return await GetMessageByIdAsync(message.Id)
            ?? throw new InvalidOperationException("Failed to retrieve created message");
    }

    public async Task<MessageDto?> UpdateMessageAsync(int id, UpdateMessageDto dto, int companyId, int userId)
    {
        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == id && m.CompanyId == companyId);
        if (message == null) return null;

        // Only creator can edit
        if (message.CreatedByUserId != userId)
        {
            throw new UnauthorizedAccessException("You can only edit your own messages.");
        }

        message.Content = dto.Content;
        message.IsEdited = true;
        message.EditedAt = DateTime.UtcNow;
        message.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetMessageByIdAsync(message.Id);
    }

    public async Task<bool> DeleteMessageAsync(int id, int companyId, int userId)
    {
        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == id && m.CompanyId == companyId);
        if (message == null) return false;

        // Only creator can delete
        if (message.CreatedByUserId != userId)
        {
            throw new UnauthorizedAccessException("You can only delete your own messages.");
        }

        // Soft delete
        message.IsDeleted = true;
        message.DeletedAt = DateTime.UtcNow;
        message.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<MessageDto> AddReactionAsync(AddReactionDto dto, int userId, int companyId)
    {
        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == dto.MessageId && m.CompanyId == companyId);
        if (message == null)
        {
            throw new InvalidOperationException("Message not found.");
        }

        var reactions = !string.IsNullOrEmpty(message.Reactions)
            ? JsonSerializer.Deserialize<Dictionary<string, List<int>>>(message.Reactions) ?? new Dictionary<string, List<int>>()
            : new Dictionary<string, List<int>>();

        if (!reactions.ContainsKey(dto.Reaction))
        {
            reactions[dto.Reaction] = new List<int>();
        }

        if (!reactions[dto.Reaction].Contains(userId))
        {
            reactions[dto.Reaction].Add(userId);
        }

        message.Reactions = JsonSerializer.Serialize(reactions);
        message.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetMessageByIdAsync(message.Id)
            ?? throw new InvalidOperationException("Failed to retrieve message");
    }

    public async Task<MessageDto> RemoveReactionAsync(RemoveReactionDto dto, int userId, int companyId)
    {
        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == dto.MessageId && m.CompanyId == companyId);
        if (message == null)
        {
            throw new InvalidOperationException("Message not found.");
        }

        var reactions = !string.IsNullOrEmpty(message.Reactions)
            ? JsonSerializer.Deserialize<Dictionary<string, List<int>>>(message.Reactions) ?? new Dictionary<string, List<int>>()
            : new Dictionary<string, List<int>>();

        if (reactions.ContainsKey(dto.Reaction))
        {
            reactions[dto.Reaction].Remove(userId);
            if (reactions[dto.Reaction].Count == 0)
            {
                reactions.Remove(dto.Reaction);
            }
        }

        message.Reactions = JsonSerializer.Serialize(reactions);
        message.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetMessageByIdAsync(message.Id)
            ?? throw new InvalidOperationException("Failed to retrieve message");
    }

    public async Task<IEnumerable<MessageDto>> GetThreadRepliesAsync(int parentMessageId, int companyId)
    {
        var replies = await _context.Messages
            .Include(m => m.CreatedByUser)
            .Include(m => m.AIAgent)
            .Where(m => m.ParentMessageId == parentMessageId && m.CompanyId == companyId && !m.IsDeleted)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                Content = m.Content,
                ContentType = m.ContentType.ToString(),
                ChannelId = m.ChannelId,
                CreatedByUserId = m.CreatedByUserId,
                CreatedByUserName = m.CreatedByUser != null ? m.CreatedByUser.Name : null,
                AIAgentId = m.AIAgentId,
                AIAgentName = m.AIAgent != null ? m.AIAgent.Name : null,
                ParentMessageId = m.ParentMessageId,
                IsEdited = m.IsEdited,
                IsDeleted = m.IsDeleted,
                EditedAt = m.EditedAt,
                DeletedAt = m.DeletedAt,
                Reactions = !string.IsNullOrEmpty(m.Reactions)
                    ? JsonSerializer.Deserialize<Dictionary<string, List<int>>>(m.Reactions)
                    : null,
                CompanyId = m.CompanyId,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
                ThreadReplyCount = 0,
                AttachmentCount = _context.MessageAttachments.Count(a => a.MessageId == m.Id)
            })
            .ToListAsync();

        return replies;
    }

    private async Task<List<MessageMentionDto>> GetMessageMentionsAsync(int messageId)
    {
        var mentions = await _context.MessageMentions
            .Include(mm => mm.MentionedUser)
            .Include(mm => mm.MentionedTeam)
            .Include(mm => mm.MentionedAIAgent)
            .Where(mm => mm.MessageId == messageId)
            .Select(mm => new MessageMentionDto
            {
                Id = mm.Id,
                MessageId = mm.MessageId,
                MentionedUserId = mm.MentionedUserId,
                MentionedUserName = mm.MentionedUser != null ? mm.MentionedUser.Name : null,
                MentionedTeamId = mm.MentionedTeamId,
                MentionedTeamName = mm.MentionedTeam != null ? mm.MentionedTeam.Name : null,
                MentionedAIAgentId = mm.MentionedAIAgentId,
                MentionedAIAgentName = mm.MentionedAIAgent != null ? mm.MentionedAIAgent.Name : null,
                IsRead = mm.IsRead,
                CreatedAt = mm.CreatedAt
            })
            .ToListAsync();

        return mentions;
    }

    private async Task<List<MessageAttachmentDto>> GetMessageAttachmentsAsync(int messageId)
    {
        var attachments = await _context.MessageAttachments
            .Include(ma => ma.UploadedByUser)
            .Where(ma => ma.MessageId == messageId)
            .Select(ma => new MessageAttachmentDto
            {
                Id = ma.Id,
                MessageId = ma.MessageId,
                FileName = ma.FileName,
                FilePath = ma.FilePath,
                FileType = ma.FileType,
                FileSize = ma.FileSize,
                UploadedByUserId = ma.UploadedByUserId,
                UploadedByUserName = ma.UploadedByUser.Name,
                CreatedAt = ma.CreatedAt
            })
            .ToListAsync();

        return attachments;
    }
}

