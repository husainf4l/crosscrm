using crm_backend.Data;
using crm_backend.Modules.Opportunity.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Opportunity.Services;

public class ProductService : IProductService
{
    private readonly CrmDbContext _context;

    public ProductService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(int? companyId = null)
    {
        var query = _context.Products
            .Include(p => p.Company)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(p => p.CompanyId == companyId.Value);
        }

        var products = await query
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                SKU = p.SKU,
                Price = p.Price,
                Currency = p.Currency,
                Cost = p.Cost,
                Type = p.Type,
                Unit = p.Unit,
                IsActive = p.IsActive,
                IsTaxable = p.IsTaxable,
                CompanyId = p.CompanyId,
                CompanyName = p.Company.Name,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        return products;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Company)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return null;

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            Price = product.Price,
            Currency = product.Currency,
            Cost = product.Cost,
            Type = product.Type,
            Unit = product.Unit,
            IsActive = product.IsActive,
            IsTaxable = product.IsTaxable,
            CompanyId = product.CompanyId,
            CompanyName = product.Company.Name,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        var companyId = dto.CompanyId ?? throw new InvalidOperationException("Company ID is required");
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId);
        if (!companyExists)
        {
            throw new InvalidOperationException($"Company with ID {companyId} does not exist.");
        }

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            SKU = dto.SKU,
            Price = dto.Price,
            Currency = dto.Currency,
            Cost = dto.Cost,
            Type = dto.Type,
            Unit = dto.Unit,
            IsActive = dto.IsActive,
            IsTaxable = dto.IsTaxable,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        await _context.Entry(product).Reference(p => p.Company).LoadAsync();

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            Price = product.Price,
            Currency = product.Currency,
            Cost = product.Cost,
            Type = product.Type,
            Unit = product.Unit,
            IsActive = product.IsActive,
            IsTaxable = product.IsTaxable,
            CompanyId = product.CompanyId,
            CompanyName = product.Company.Name,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var product = await _context.Products
            .Include(p => p.Company)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            product.Name = dto.Name;
        }

        if (dto.Description != null)
        {
            product.Description = dto.Description;
        }

        if (dto.SKU != null)
        {
            product.SKU = dto.SKU;
        }

        if (dto.Price.HasValue)
        {
            product.Price = dto.Price.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.Currency))
        {
            product.Currency = dto.Currency;
        }

        if (dto.Cost.HasValue)
        {
            product.Cost = dto.Cost;
        }

        if (dto.Type.HasValue)
        {
            product.Type = dto.Type.Value;
        }

        if (dto.Unit != null)
        {
            product.Unit = dto.Unit;
        }

        if (dto.IsActive.HasValue)
        {
            product.IsActive = dto.IsActive.Value;
        }

        if (dto.IsTaxable.HasValue)
        {
            product.IsTaxable = dto.IsTaxable.Value;
        }

        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            Price = product.Price,
            Currency = product.Currency,
            Cost = product.Cost,
            Type = product.Type,
            Unit = product.Unit,
            IsActive = product.IsActive,
            IsTaxable = product.IsTaxable,
            CompanyId = product.CompanyId,
            CompanyName = product.Company.Name,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        // Check if any opportunity products are using this product
        var hasOpportunityProducts = await _context.OpportunityProducts
            .AnyAsync(op => op.ProductId == id);
        
        if (hasOpportunityProducts)
        {
            throw new InvalidOperationException("Cannot delete product that has associated opportunity products.");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
}

