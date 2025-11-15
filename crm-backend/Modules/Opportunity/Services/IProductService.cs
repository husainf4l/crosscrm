using crm_backend.Modules.Opportunity.DTOs;

namespace crm_backend.Modules.Opportunity.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync(int? companyId = null);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteProductAsync(int id);
}

