using crm_backend.Modules.Communication.DTOs;

namespace crm_backend.Modules.Communication.Services;

public interface IEmailService
{
    Task<IEnumerable<EmailDto>> GetAllEmailsAsync(int? companyId = null);
    Task<EmailDto?> GetEmailByIdAsync(int id);
    Task<EmailDto> CreateEmailAsync(CreateEmailDto dto);
    Task<EmailDto?> UpdateEmailAsync(int id, UpdateEmailDto dto);
    Task<bool> DeleteEmailAsync(int id);
    Task<EmailDto?> MarkEmailAsReadAsync(int id, bool isRead);
}

