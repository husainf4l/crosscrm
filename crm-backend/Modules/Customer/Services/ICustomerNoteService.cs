using crm_backend.Modules.Customer.DTOs;

namespace crm_backend.Modules.Customer.Services;

public interface ICustomerNoteService
{
    Task<IEnumerable<CustomerNoteDto>> GetAllNotesAsync(int customerId, int userId, bool includePrivate = false);
    Task<CustomerNoteDto?> GetNoteByIdAsync(int id, int userId, bool includePrivate = false);
    Task<CustomerNoteDto> CreateNoteAsync(CreateCustomerNoteDto noteDto, int createdByUserId);
    Task<CustomerNoteDto?> UpdateNoteAsync(int id, UpdateCustomerNoteDto noteDto, int userId);
    Task<bool> DeleteNoteAsync(int id, int userId);
    Task<IEnumerable<CustomerNoteDto>> GetNotesByTypeAsync(int customerId, NoteType type, int userId, bool includePrivate = false);
}
