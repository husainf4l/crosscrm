using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.Services;

public interface ICustomerStrategyService
{
    Task<IEnumerable<CustomerStrategyDto>> GetStrategiesByCustomerAsync(int customerId, int companyId);
    Task<IEnumerable<CustomerStrategyDto>> GetStrategiesByWorkspaceAsync(int workspaceId, int companyId);
    Task<CustomerStrategyDto?> GetStrategyByIdAsync(int id);
    Task<CustomerStrategyDto> CreateStrategyAsync(CreateCustomerStrategyDto dto, int companyId, int createdByUserId);
    Task<CustomerStrategyDto?> UpdateStrategyAsync(int id, UpdateCustomerStrategyDto dto, int companyId);
    Task<bool> DeleteStrategyAsync(int id, int companyId);
}

