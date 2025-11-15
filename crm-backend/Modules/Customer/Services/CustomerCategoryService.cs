using crm_backend.Data;
using crm_backend.Modules.Customer.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Customer.Services;

public class CustomerCategoryService : ICustomerCategoryService
{
    private readonly CrmDbContext _context;

    public CustomerCategoryService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerCategoryDto> CreateCategoryAsync(CreateCustomerCategoryDto dto, int userId)
    {
        // Check if category name already exists for this company
        var existingCategory = await _context.CustomerCategories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == dto.Name.ToLower());

        if (existingCategory != null)
            throw new Exception("Category with this name already exists");

        var category = new CustomerCategory
        {
            Name = dto.Name,
            Description = dto.Description,
            Color = dto.Color
        };

        _context.CustomerCategories.Add(category);
        await _context.SaveChangesAsync();

        return await GetCategoryDtoAsync(category.Id);
    }

    public async Task<CustomerCategoryDto?> UpdateCategoryAsync(int categoryId, UpdateCustomerCategoryDto dto, int userId)
    {
        var category = await _context.CustomerCategories
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null)
            return null;

        if (dto.Name != null)
        {
            // Check for name conflicts
            var existingCategory = await _context.CustomerCategories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == dto.Name.ToLower() && c.Id != categoryId);

            if (existingCategory != null)
                throw new Exception("Category with this name already exists");

            category.Name = dto.Name;
        }

        if (dto.Description != null)
            category.Description = dto.Description;

        if (dto.Color != null)
            category.Color = dto.Color;

        if (dto.IsActive.HasValue)
            category.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();

        return await GetCategoryDtoAsync(categoryId);
    }

    public async Task<bool> DeleteCategoryAsync(int categoryId, int userId)
    {
        var category = await _context.CustomerCategories
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null)
            return false;

        // Check if category is being used
        var mappingsCount = await _context.CustomerCategoryMappings
            .CountAsync(ccm => ccm.CategoryId == categoryId);

        if (mappingsCount > 0)
            throw new Exception("Cannot delete category that is assigned to customers");

        _context.CustomerCategories.Remove(category);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<CustomerCategoryDto>> GetCategoriesAsync(int userId)
    {
        var categories = await _context.CustomerCategories
            .Where(c => c.IsActive)
            .ToListAsync();

        var categoryDtos = new List<CustomerCategoryDto>();

        foreach (var category in categories)
        {
            var customerCount = await _context.CustomerCategoryMappings
                .CountAsync(ccm => ccm.CategoryId == category.Id);

            categoryDtos.Add(new CustomerCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Color = category.Color,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                CustomerCount = customerCount
            });
        }

        return categoryDtos;
    }

    public async Task<CustomerCategoryDto?> GetCategoryAsync(int categoryId, int userId)
    {
        var category = await _context.CustomerCategories
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.IsActive);

        if (category == null)
            return null;

        var customerCount = await _context.CustomerCategoryMappings
            .CountAsync(ccm => ccm.CategoryId == categoryId);

        return new CustomerCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Color = category.Color,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            CustomerCount = customerCount
        };
    }

    public async Task<CustomerCategoryMappingDto> AssignCustomerToCategoryAsync(AssignCustomerToCategoryDto dto, int userId)
    {
        // Verify customer exists and user has access
        var customer = await _context.Customers
            .Include(c => c.Company)
            .FirstOrDefaultAsync(c => c.Id == dto.CustomerId);

        if (customer == null)
            throw new Exception("Customer not found");

        var userCompany = await _context.UserCompanies
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompanyId == customer.CompanyId && uc.IsActive);

        if (userCompany == null)
            throw new Exception("Access denied");

        // Verify category exists
        var category = await _context.CustomerCategories
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.IsActive);

        if (category == null)
            throw new Exception("Category not found");

        // Check if mapping already exists
        var existingMapping = await _context.CustomerCategoryMappings
            .FirstOrDefaultAsync(ccm => ccm.CustomerId == dto.CustomerId && ccm.CategoryId == dto.CategoryId);

        if (existingMapping != null)
            throw new Exception("Customer is already assigned to this category");

        var mapping = new CustomerCategoryMapping
        {
            CustomerId = dto.CustomerId,
            CategoryId = dto.CategoryId,
            AssignedByUserId = userId,
            Notes = dto.Notes
        };

        _context.CustomerCategoryMappings.Add(mapping);
        await _context.SaveChangesAsync();

        // Return the mapping DTO
        var user = await _context.Users.FindAsync(userId);
        return new CustomerCategoryMappingDto
        {
            CustomerId = dto.CustomerId,
            CustomerName = customer.Name,
            CategoryId = dto.CategoryId,
            CategoryName = category.Name,
            CategoryColor = category.Color,
            AssignedByUserId = userId,
            AssignedByUserName = user?.Name ?? "Unknown",
            AssignedAt = mapping.AssignedAt,
            Notes = dto.Notes
        };
    }

    public async Task<bool> RemoveCustomerFromCategoryAsync(int customerId, int categoryId, int userId)
    {
        // Verify access
        var customer = await _context.Customers
            .Include(c => c.Company)
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null)
            throw new Exception("Customer not found");

        var userCompany = await _context.UserCompanies
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompanyId == customer.CompanyId && uc.IsActive);

        if (userCompany == null)
            throw new Exception("Access denied");

        var mapping = await _context.CustomerCategoryMappings
            .FirstOrDefaultAsync(ccm => ccm.CustomerId == customerId && ccm.CategoryId == categoryId);

        if (mapping == null)
            return false;

        _context.CustomerCategoryMappings.Remove(mapping);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<CustomerCategoryDto>> GetCustomerCategoriesAsync(int customerId, int userId)
    {
        // Verify access
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null)
            throw new Exception("Customer not found");

        var userCompany = await _context.UserCompanies
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompanyId == customer.CompanyId && uc.IsActive);

        if (userCompany == null)
            throw new Exception("Access denied");

        var categories = await _context.CustomerCategoryMappings
            .Where(ccm => ccm.CustomerId == customerId)
            .Include(ccm => ccm.Category)
            .Select(ccm => ccm.Category)
            .Where(c => c.IsActive)
            .ToListAsync();

        return categories.Select(c => new CustomerCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Color = c.Color,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            CustomerCount = 0 // Not needed for this context
        });
    }

    public async Task<IEnumerable<CustomerDto>> GetCustomersByCategoryAsync(int categoryId, int userId)
    {
        // Get user's company
        var userCompany = await _context.UserCompanies
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.IsActive);

        if (userCompany == null)
            throw new Exception("User not associated with any company");

        var customers = await _context.Customers
            .Where(c => c.CompanyId == userCompany.CompanyId)
            .Where(c => c.CategoryMappings.Any(ccm => ccm.CategoryId == categoryId))
            .Include(c => c.Company)
            .Include(c => c.CategoryMappings)
                .ThenInclude(ccm => ccm.Category)
            .ToListAsync();

        return customers.Select(c => new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            Phone = c.Phone,
            Address = c.Address,
            City = c.City,
            Country = c.Country,
            Latitude = c.Latitude,
            Longitude = c.Longitude,
            Status = c.Status,
            CompanyId = c.CompanyId,
            CompanyName = c.Company != null ? c.Company.Name : string.Empty,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            Categories = c.CategoryMappings.Select(ccm => new CustomerCategoryDto
            {
                Id = ccm.Category.Id,
                Name = ccm.Category.Name,
                Description = ccm.Category.Description,
                Color = ccm.Category.Color,
                IsActive = ccm.Category.IsActive,
                CreatedAt = ccm.Category.CreatedAt,
                CustomerCount = 0 // We'll calculate this separately if needed
            }).ToList(),
            TotalActivities = c.ActivityLogs.Count,
            LastActivity = c.ActivityLogs.Any() ? c.ActivityLogs.Max(a => a.CreatedAt) : null
        });
    }

    private async Task<CustomerCategoryDto> GetCategoryDtoAsync(int id)
    {
        var category = await _context.CustomerCategories
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            throw new Exception("Category not found");

        var customerCount = await _context.CustomerCategoryMappings
            .CountAsync(ccm => ccm.CategoryId == id);

        return new CustomerCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Color = category.Color,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            CustomerCount = customerCount
        };
    }
}