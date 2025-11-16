using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.Services;

public interface IMessageService
{
    Task<IEnumerable<MessageDto>> GetMessagesByChannelAsync(int channelId, int companyId, int? userId = null, int skip = 0, int take = 50);
    Task<MessageDto?> GetMessageByIdAsync(int id, int? userId = null);
    Task<MessageDto> SendMessageAsync(CreateMessageDto dto, int companyId, int? userId = null);
    Task<MessageDto?> UpdateMessageAsync(int id, UpdateMessageDto dto, int companyId, int userId);
    Task<bool> DeleteMessageAsync(int id, int companyId, int userId);
    Task<MessageDto> AddReactionAsync(AddReactionDto dto, int userId, int companyId);
    Task<MessageDto> RemoveReactionAsync(RemoveReactionDto dto, int userId, int companyId);
    Task<IEnumerable<MessageDto>> GetThreadRepliesAsync(int parentMessageId, int companyId);
}

