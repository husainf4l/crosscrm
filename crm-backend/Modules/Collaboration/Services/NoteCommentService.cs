using crm_backend.Data;
using crm_backend.Modules.Collaboration.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Collaboration.Services;

public class NoteCommentService : INoteCommentService
{
    private readonly CrmDbContext _context;

    public NoteCommentService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<NoteCommentDto>> GetCommentsByEntityAsync(string entityType, int entityId, int companyId)
    {
        var comments = await _context.NoteComments
            .Include(nc => nc.CreatedByUser)
            .Where(nc => nc.EntityType == entityType && nc.EntityId == entityId && nc.CompanyId == companyId && nc.ParentCommentId == null)
            .OrderBy(nc => nc.CreatedAt)
            .Select(nc => new NoteCommentDto
            {
                Id = nc.Id,
                Content = nc.Content,
                EntityType = nc.EntityType,
                EntityId = nc.EntityId,
                ParentCommentId = nc.ParentCommentId,
                CreatedByUserId = nc.CreatedByUserId,
                CreatedByUserName = nc.CreatedByUser.Name,
                CreatedAt = nc.CreatedAt,
                UpdatedAt = nc.UpdatedAt,
                ReplyCount = _context.NoteComments.Count(r => r.ParentCommentId == nc.Id)
            })
            .ToListAsync();

        return comments;
    }

    public async Task<NoteCommentDto?> GetCommentByIdAsync(int id)
    {
        var comment = await _context.NoteComments
            .Include(nc => nc.CreatedByUser)
            .FirstOrDefaultAsync(nc => nc.Id == id);

        if (comment == null) return null;

        var replyCount = await _context.NoteComments
            .CountAsync(r => r.ParentCommentId == comment.Id);

        return new NoteCommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            EntityType = comment.EntityType,
            EntityId = comment.EntityId,
            ParentCommentId = comment.ParentCommentId,
            CreatedByUserId = comment.CreatedByUserId,
            CreatedByUserName = comment.CreatedByUser.Name,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            ReplyCount = replyCount
        };
    }

    public async Task<NoteCommentDto> CreateCommentAsync(CreateNoteCommentDto dto, int companyId, int createdByUserId)
    {
        // Verify entity exists based on type
        bool entityExists = dto.EntityType.ToLower() switch
        {
            "note" => await _context.CustomerNotes.AnyAsync(n => n.Id == dto.EntityId),
            "strategy" => await _context.CustomerStrategies.AnyAsync(s => s.Id == dto.EntityId && s.CompanyId == companyId),
            "idea" => await _context.CustomerIdeas.AnyAsync(i => i.Id == dto.EntityId && i.CompanyId == companyId),
            _ => false
        };

        if (!entityExists)
        {
            throw new InvalidOperationException($"Entity of type {dto.EntityType} with ID {dto.EntityId} not found.");
        }

        // Verify parent comment exists (if provided)
        if (dto.ParentCommentId.HasValue)
        {
            var parentComment = await _context.NoteComments
                .FirstOrDefaultAsync(nc => nc.Id == dto.ParentCommentId.Value && nc.CompanyId == companyId);
            if (parentComment == null)
            {
                throw new InvalidOperationException("Parent comment not found or does not belong to the company.");
            }
        }

        var comment = new NoteComment
        {
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            ParentCommentId = dto.ParentCommentId,
            Content = dto.Content,
            CreatedByUserId = createdByUserId,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.NoteComments.Add(comment);
        await _context.SaveChangesAsync();

        return await GetCommentByIdAsync(comment.Id)
            ?? throw new InvalidOperationException("Failed to retrieve created comment");
    }

    public async Task<NoteCommentDto?> UpdateCommentAsync(int id, UpdateNoteCommentDto dto, int companyId, int userId)
    {
        var comment = await _context.NoteComments
            .FirstOrDefaultAsync(nc => nc.Id == id && nc.CompanyId == companyId);
        if (comment == null) return null;

        // Only creator can update
        if (comment.CreatedByUserId != userId)
        {
            throw new UnauthorizedAccessException("You can only update your own comments.");
        }

        comment.Content = dto.Content;
        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetCommentByIdAsync(comment.Id);
    }

    public async Task<bool> DeleteCommentAsync(int id, int companyId, int userId)
    {
        var comment = await _context.NoteComments
            .FirstOrDefaultAsync(nc => nc.Id == id && nc.CompanyId == companyId);
        if (comment == null) return false;

        // Only creator can delete
        if (comment.CreatedByUserId != userId)
        {
            throw new UnauthorizedAccessException("You can only delete your own comments.");
        }

        // Check if comment has replies
        var hasReplies = await _context.NoteComments.AnyAsync(nc => nc.ParentCommentId == id);
        if (hasReplies)
        {
            // Soft delete: just clear content
            comment.Content = "[Deleted]";
            comment.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Hard delete if no replies
            _context.NoteComments.Remove(comment);
        }

        await _context.SaveChangesAsync();

        return true;
    }
}

