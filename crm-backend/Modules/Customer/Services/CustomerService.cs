using crm_backend.Data;
using crm_backend.Modules.Customer.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text;

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
                UpdatedAt = c.UpdatedAt,
                Status = c.Status,
                ContactPersonName = c.ContactPersonName,
                CustomerType = c.CustomerType,
                Industry = c.Industry,
                Website = c.Website,
                Priority = c.Priority
            })
            .ToListAsync();

        return customers;
    }

    public async Task<CustomerConnectionDto> GetCustomersConnectionAsync(
        int companyId, 
        int? first = null, 
        string? after = null,
        string? search = null,
        CustomerFiltersDto? filters = null)
    {
        var query = _context.Customers
            .Include(c => c.Company)
            .Where(c => c.CompanyId == companyId);

        // Apply global search across multiple fields
        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(c => 
                c.Name.ToLower().Contains(searchLower) ||
                (c.Email != null && c.Email.ToLower().Contains(searchLower)) ||
                (c.Phone != null && c.Phone.ToLower().Contains(searchLower)) ||
                (c.ContactPersonName != null && c.ContactPersonName.ToLower().Contains(searchLower)) ||
                (c.City != null && c.City.ToLower().Contains(searchLower)) ||
                (c.Country != null && c.Country.ToLower().Contains(searchLower)) ||
                (c.Industry != null && c.Industry.ToLower().Contains(searchLower)) ||
                (c.Website != null && c.Website.ToLower().Contains(searchLower))
            );
        }

        // Apply filters
        if (filters != null)
        {
            if (!string.IsNullOrEmpty(filters.Name))
                query = query.Where(c => c.Name.ToLower().Contains(filters.Name.ToLower()));
            
            if (!string.IsNullOrEmpty(filters.Email))
                query = query.Where(c => c.Email != null && c.Email.ToLower().Contains(filters.Email.ToLower()));
            
            if (!string.IsNullOrEmpty(filters.Status))
                query = query.Where(c => c.Status == filters.Status);
            
            if (!string.IsNullOrEmpty(filters.City))
                query = query.Where(c => c.City != null && c.City.ToLower().Contains(filters.City.ToLower()));
            
            if (!string.IsNullOrEmpty(filters.Country))
                query = query.Where(c => c.Country != null && c.Country.ToLower().Contains(filters.Country.ToLower()));
            
            if (!string.IsNullOrEmpty(filters.CustomerType))
                query = query.Where(c => c.CustomerType == filters.CustomerType);
            
            if (!string.IsNullOrEmpty(filters.Industry))
                query = query.Where(c => c.Industry != null && c.Industry.ToLower().Contains(filters.Industry.ToLower()));
            
            if (!string.IsNullOrEmpty(filters.Priority))
                query = query.Where(c => c.Priority == filters.Priority);
            
            if (filters.CreatedFrom.HasValue)
                query = query.Where(c => c.CreatedAt >= filters.CreatedFrom.Value);
            
            if (filters.CreatedTo.HasValue)
                query = query.Where(c => c.CreatedAt <= filters.CreatedTo.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Handle cursor-based pagination
        if (!string.IsNullOrEmpty(after))
        {
            var afterId = DecodeCursor(after);
            query = query.Where(c => c.Id > afterId);
        }

        // Default first to 10, max 100
        var take = Math.Min(first ?? 10, 100);
        
        // Order by Id for consistent pagination
        var customers = await query
            .OrderBy(c => c.Id)
            .Take(take + 1) // Take one extra to check if there's a next page
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
                UpdatedAt = c.UpdatedAt,
                Status = c.Status,
                ContactPersonName = c.ContactPersonName,
                CustomerType = c.CustomerType,
                Industry = c.Industry,
                Website = c.Website,
                Priority = c.Priority
            })
            .ToListAsync();

        var hasNextPage = customers.Count > take;
        if (hasNextPage)
        {
            customers = customers.Take(take).ToList();
        }

        var edges = customers.Select(c => new CustomerEdgeDto
        {
            Node = c,
            Cursor = EncodeCursor(c.Id)
        }).ToList();

        return new CustomerConnectionDto
        {
            Edges = edges,
            PageInfo = new PageInfoDto
            {
                HasNextPage = hasNextPage,
                HasPreviousPage = !string.IsNullOrEmpty(after),
                StartCursor = edges.FirstOrDefault()?.Cursor,
                EndCursor = edges.LastOrDefault()?.Cursor
            },
            TotalCount = totalCount
        };
    }

    private string EncodeCursor(int id)
    {
        var bytes = Encoding.UTF8.GetBytes(id.ToString());
        return Convert.ToBase64String(bytes);
    }

    private int DecodeCursor(string cursor)
    {
        try
        {
            var bytes = Convert.FromBase64String(cursor);
            var idString = Encoding.UTF8.GetString(bytes);
            return int.Parse(idString);
        }
        catch
        {
            return 0; // Default to 0 if cursor is invalid
        }
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
