using crm_backend.Modules.Customer.DTOs;

namespace crm_backend.Modules.Customer.Services;

public interface ICustomerCategoryService
{
    Task<CustomerCategoryDto> CreateCategoryAsync(CreateCustomerCategoryDto dto, int userId);
    Task<CustomerCategoryDto?> UpdateCategoryAsync(int categoryId, UpdateCustomerCategoryDto dto, int userId);
    Task<bool> DeleteCategoryAsync(int categoryId, int userId);
    Task<IEnumerable<CustomerCategoryDto>> GetCategoriesAsync(int userId);
    Task<CustomerCategoryDto?> GetCategoryAsync(int categoryId, int userId);
    Task<CustomerCategoryMappingDto> AssignCustomerToCategoryAsync(AssignCustomerToCategoryDto dto, int userId);
    Task<bool> RemoveCustomerFromCategoryAsync(int customerId, int categoryId, int userId);
    Task<IEnumerable<CustomerCategoryDto>> GetCustomerCategoriesAsync(int customerId, int userId);
    Task<IEnumerable<CustomerDto>> GetCustomersByCategoryAsync(int categoryId, int userId);
}
