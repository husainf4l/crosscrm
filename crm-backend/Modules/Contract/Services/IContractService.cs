using crm_backend.Modules.Contract.DTOs;

namespace crm_backend.Modules.Contract.Services;

public interface IContractService
{
    Task<IEnumerable<ContractDto>> GetAllContractsAsync(int? companyId = null);
    Task<ContractDto?> GetContractByIdAsync(int id);
    Task<ContractDto> CreateContractAsync(CreateContractDto dto);
    Task<ContractDto?> UpdateContractAsync(int id, UpdateContractDto dto);
    Task<bool> DeleteContractAsync(int id);
    Task<ContractDto> RenewContractAsync(int id);
    Task<IEnumerable<ContractDto>> GetExpiringContractsAsync(int companyId, int daysAhead = 30);
    Task<IEnumerable<ContractDto>> GetExpiredContractsAsync(int companyId);
}

