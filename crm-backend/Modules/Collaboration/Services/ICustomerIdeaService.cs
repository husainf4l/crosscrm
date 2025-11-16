using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.Services;

public interface ICustomerIdeaService
{
    Task<IEnumerable<CustomerIdeaDto>> GetIdeasByCustomerAsync(int customerId, int companyId, int? userId = null);
    Task<IEnumerable<CustomerIdeaDto>> GetIdeasByWorkspaceAsync(int workspaceId, int companyId, int? userId = null);
    Task<CustomerIdeaDto?> GetIdeaByIdAsync(int id, int? userId = null);
    Task<CustomerIdeaDto> CreateIdeaAsync(CreateCustomerIdeaDto dto, int companyId, int createdByUserId);
    Task<CustomerIdeaDto?> UpdateIdeaAsync(int id, UpdateCustomerIdeaDto dto, int companyId);
    Task<bool> DeleteIdeaAsync(int id, int companyId);
    Task<CustomerIdeaDto> VoteIdeaAsync(VoteIdeaDto dto, int userId, int companyId);
    Task<bool> RemoveVoteAsync(int ideaId, int userId, int companyId);
}

