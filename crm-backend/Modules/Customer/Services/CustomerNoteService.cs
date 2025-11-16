using crm_backend.Data;
using crm_backend.Modules.Customer.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Customer.Services;

public class CustomerNoteService : ICustomerNoteService
{
    private readonly CrmDbContext _context;

    public CustomerNoteService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomerNoteDto>> GetAllNotesAsync(int customerId, int userId, bool includePrivate = false)
    {
        var query = _context.CustomerNotes
            .Include(n => n.CreatedByUser)
            .Where(n => n.CustomerId == customerId);

        // If not including private notes, only show public notes or notes created by the current user
        if (!includePrivate)
        {
            query = query.Where(n => !n.IsPrivate || n.CreatedByUserId == userId);
        }

        var notes = await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notes.Select(MapToDto);
    }

    public async Task<CustomerNoteDto?> GetNoteByIdAsync(int id, int userId, bool includePrivate = false)
    {
        var query = _context.CustomerNotes
            .Include(n => n.CreatedByUser)
            .Where(n => n.Id == id);

        // If not including private notes, only show public notes or notes created by the current user
        if (!includePrivate)
        {
            query = query.Where(n => !n.IsPrivate || n.CreatedByUserId == userId);
        }

        var note = await query.FirstOrDefaultAsync();
        return note != null ? MapToDto(note) : null;
    }

    public async Task<CustomerNoteDto> CreateNoteAsync(CreateCustomerNoteDto noteDto, int createdByUserId)
    {
        var note = new CustomerNote
        {
            Content = noteDto.Content,
            Type = (NoteType)noteDto.Type,
            CustomerId = noteDto.CustomerId,
            CreatedByUserId = createdByUserId,
            IsPrivate = noteDto.IsPrivate,
            Tags = noteDto.Tags,
            CreatedAt = DateTime.UtcNow
        };

        _context.CustomerNotes.Add(note);
        await _context.SaveChangesAsync();

        return await GetNoteByIdAsync(note.Id, createdByUserId, true) ?? throw new InvalidOperationException("Failed to create note");
    }

    public async Task<CustomerNoteDto?> UpdateNoteAsync(int id, UpdateCustomerNoteDto noteDto, int userId)
    {
        var note = await _context.CustomerNotes.FindAsync(id);
        if (note == null)
        {
            return null;
        }

        // Only allow the creator to update their notes
        if (note.CreatedByUserId != userId)
        {
            throw new UnauthorizedAccessException("You can only update your own notes");
        }

        if (noteDto.Content != null) note.Content = noteDto.Content;
        if (noteDto.Type.HasValue) note.Type = (NoteType)noteDto.Type.Value;
        if (noteDto.IsPrivate.HasValue) note.IsPrivate = noteDto.IsPrivate.Value;
        if (noteDto.Tags != null) note.Tags = noteDto.Tags;

        note.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetNoteByIdAsync(id, userId, true);
    }

    public async Task<bool> DeleteNoteAsync(int id, int userId)
    {
        var note = await _context.CustomerNotes.FindAsync(id);
        if (note == null)
        {
            return false;
        }

        // Only allow the creator to delete their notes
        if (note.CreatedByUserId != userId)
        {
            throw new UnauthorizedAccessException("You can only delete your own notes");
        }

        _context.CustomerNotes.Remove(note);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<CustomerNoteDto>> GetNotesByTypeAsync(int customerId, NoteType type, int userId, bool includePrivate = false)
    {
        var query = _context.CustomerNotes
            .Include(n => n.CreatedByUser)
            .Where(n => n.CustomerId == customerId && n.Type == type);

        // If not including private notes, only show public notes or notes created by the current user
        if (!includePrivate)
        {
            query = query.Where(n => !n.IsPrivate || n.CreatedByUserId == userId);
        }

        var notes = await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notes.Select(MapToDto);
    }

    private static CustomerNoteDto MapToDto(CustomerNote note)
    {
        return new CustomerNoteDto
        {
            Id = note.Id,
            Content = note.Content,
            Type = (DTOs.NoteType)note.Type,
            CustomerId = note.CustomerId,
            CreatedByUserId = note.CreatedByUserId,
            CreatedByUserName = note.CreatedByUser?.Name,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt,
            IsPrivate = note.IsPrivate,
            Tags = note.Tags
        };
    }
}
