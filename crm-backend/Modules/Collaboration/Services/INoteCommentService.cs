using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.Services;

public interface INoteCommentService
{
    Task<IEnumerable<NoteCommentDto>> GetCommentsByEntityAsync(string entityType, int entityId, int companyId);
    Task<NoteCommentDto?> GetCommentByIdAsync(int id);
    Task<NoteCommentDto> CreateCommentAsync(CreateNoteCommentDto dto, int companyId, int createdByUserId);
    Task<NoteCommentDto?> UpdateCommentAsync(int id, UpdateNoteCommentDto dto, int companyId, int userId);
    Task<bool> DeleteCommentAsync(int id, int companyId, int userId);
}

