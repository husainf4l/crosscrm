using crm_backend.Data;
using crm_backend.Modules.Customer.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Customer.Services;

public class CustomerService : ICustomerService
{
    private readonly CrmDbContext _context;

    public CustomerService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(int? companyId = null)
    {
        var query = _context.Customers
            .Include(c => c.Company)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(c => c.CompanyId == companyId.Value);
        }

        var customers = await query
            .Select(c => new CustomerDto
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
                CompanyId = c.CompanyId,
                CompanyName = c.Company != null ? c.Company.Name : string.Empty,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return customers;
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        var customer = await _context.Customers
            .Include(c => c.Company)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null) return null;

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            City = customer.City,
            Country = customer.Country,
            Latitude = customer.Latitude,
            Longitude = customer.Longitude,
            CompanyId = customer.CompanyId,
            CompanyName = customer.Company != null ? customer.Company.Name : string.Empty,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto)
    {
        // Verify company exists
        var companyId = dto.CompanyId ?? throw new InvalidOperationException("Company ID is required");
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId);
        if (!companyExists)
        {
            throw new InvalidOperationException($"Company with ID {companyId} does not exist.");
        }

        var customer = new Customer
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            Country = dto.Country,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Load company for response
        await _context.Entry(customer).Reference(c => c.Company).LoadAsync();

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            City = customer.City,
            Country = customer.Country,
            Latitude = customer.Latitude,
            Longitude = customer.Longitude,
            CompanyId = customer.CompanyId,
            CompanyName = customer.Company != null ? customer.Company.Name : string.Empty,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }

    public async Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto dto)
    {
        var customer = await _context.Customers
            .Include(c => c.Company)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null) return null;

        // Only update fields that are provided (not null/empty for required fields)
        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            customer.Name = dto.Name;
        }

        if (dto.Email != null)
        {
            customer.Email = dto.Email;
        }

        if (dto.Phone != null)
        {
            customer.Phone = dto.Phone;
        }

        if (dto.Address != null)
        {
            customer.Address = dto.Address;
        }

        if (dto.City != null)
        {
            customer.City = dto.City;
        }

        if (dto.Country != null)
        {
            customer.Country = dto.Country;
        }

        if (dto.Latitude.HasValue)
        {
            customer.Latitude = dto.Latitude.Value;
        }

        if (dto.Longitude.HasValue)
        {
            customer.Longitude = dto.Longitude.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            customer.Status = dto.Status;
        }

        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            City = customer.City,
            Country = customer.Country,
            Latitude = customer.Latitude,
            Longitude = customer.Longitude,
            Status = customer.Status,
            CompanyId = customer.CompanyId,
            CompanyName = customer.Company != null ? customer.Company.Name : string.Empty,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return false;

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return true;
    }
}
