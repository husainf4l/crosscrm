using crm_backend.Modules.Customer.DTOs;

namespace crm_backend.Modules.Customer.Services;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(int? companyId = null);
    Task<CustomerConnectionDto> GetCustomersConnectionAsync(
        int companyId, 
        int? first = null, 
        string? after = null,
        string? search = null,
        CustomerFiltersDto? filters = null);
    Task<CustomerDto?> GetCustomerByIdAsync(int id);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto);
    Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto dto);
    Task<bool> DeleteCustomerAsync(int id);
}
