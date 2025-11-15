using crm_backend.Modules.Customer.DTOs;

namespace crm_backend.Modules.Customer.Services;

public interface ICustomerPreferenceService
{
    Task<CustomerPreferenceDto> CreatePreferenceAsync(CreateCustomerPreferenceDto dto, int userId);
    Task<CustomerPreferenceDto?> UpdatePreferenceAsync(int preferenceId, UpdateCustomerPreferenceDto dto, int userId);
    Task<bool> DeletePreferenceAsync(int preferenceId, int userId);
    Task<IEnumerable<CustomerPreferenceDto>> GetCustomerPreferencesAsync(int customerId, int userId);
    Task<CustomerPreferenceDto?> GetPreferenceAsync(int preferenceId, int userId);
    Task<CustomerPreferenceDto?> GetCustomerPreferenceByKeyAsync(int customerId, string key, int userId);
}