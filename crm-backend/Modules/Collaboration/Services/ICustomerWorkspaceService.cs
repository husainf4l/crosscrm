using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.Services;

public interface ICustomerWorkspaceService
{
    Task<CustomerWorkspaceDto?> GetWorkspaceByCustomerIdAsync(int customerId, int companyId);
    Task<CustomerWorkspaceDto> GetOrCreateWorkspaceAsync(int customerId, int companyId);
    Task<CustomerWorkspaceDto> CreateWorkspaceAsync(CreateCustomerWorkspaceDto dto, int companyId);
    Task<CustomerWorkspaceDto?> UpdateWorkspaceAsync(int id, UpdateCustomerWorkspaceDto dto, int companyId, int? updatedByUserId = null);
    Task<bool> DeleteWorkspaceAsync(int id, int companyId);
}

