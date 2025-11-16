using crm_backend.Data;
using crm_backend.Modules.Company.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Company.Services;

public class CompanyService : ICompanyService
{
    private readonly CrmDbContext _context;

    public CompanyService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CompanyDto>> GetAllCompaniesAsync()
    {
        var companies = await _context.Companies
            .Include(c => c.UserCompanies)
            .Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Website = c.Website,
                Industry = c.Industry,
                Size = c.Size,
                Address = c.Address,
                City = c.City,
                State = c.State,
                PostalCode = c.PostalCode,
                Country = c.Country,
                Phone = c.Phone,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                UserCount = c.UserCompanies.Count
            })
            .ToListAsync();

        return companies;
    }

    public async Task<CompanyDto?> GetCompanyByIdAsync(int id)
    {
        var company = await _context.Companies
            .Include(c => c.UserCompanies)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (company == null) return null;

        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Description = company.Description,
            Email = company.Email,
            Logo = company.Logo,
            Website = company.Website,
            Industry = company.Industry,
            Size = company.Size,
            Address = company.Address,
            City = company.City,
            State = company.State,
            PostalCode = company.PostalCode,
            Country = company.Country,
            Phone = company.Phone,
            CreatedAt = company.CreatedAt,
            UpdatedAt = company.UpdatedAt,
            UserCount = company.UserCompanies.Count
        };
    }

    public async Task<CompanyDto> CreateCompanyAsync(CreateCompanyDto dto)
    {
        var company = new Company
        {
            Name = dto.Name,
            Description = dto.Description,
            Email = dto.Email,
            Logo = dto.Logo,
            Website = dto.Website,
            Industry = dto.Industry,
            Size = dto.Size,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
            Phone = dto.Phone
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Description = company.Description,
            Email = company.Email,
            Logo = company.Logo,
            Website = company.Website,
            Industry = company.Industry,
            Size = company.Size,
            Address = company.Address,
            City = company.City,
            State = company.State,
            PostalCode = company.PostalCode,
            Country = company.Country,
            Phone = company.Phone,
            CreatedAt = company.CreatedAt,
            UpdatedAt = company.UpdatedAt,
            UserCount = 0
        };
    }

    public async Task<CompanyDto?> UpdateCompanyAsync(int id, UpdateCompanyDto dto)
    {
        var company = await _context.Companies
            .Include(c => c.UserCompanies)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (company == null) return null;

        company.Name = dto.Name;
        company.Description = dto.Description;
        company.Email = dto.Email;
        company.Logo = dto.Logo;
        company.Website = dto.Website;
        company.Industry = dto.Industry;
        company.Size = dto.Size;
        company.Address = dto.Address;
        company.City = dto.City;
        company.State = dto.State;
        company.PostalCode = dto.PostalCode;
        company.Country = dto.Country;
        company.Phone = dto.Phone;
        company.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Description = company.Description,
            Website = company.Website,
            Industry = company.Industry,
            Size = company.Size,
            Address = company.Address,
            City = company.City,
            State = company.State,
            PostalCode = company.PostalCode,
            Country = company.Country,
            Phone = company.Phone,
            CreatedAt = company.CreatedAt,
            UpdatedAt = company.UpdatedAt,
            UserCount = company.UserCompanies.Count
        };
    }

    public async Task<bool> DeleteCompanyAsync(int id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return false;

        _context.Companies.Remove(company);
        await _context.SaveChangesAsync();
        return true;
    }
}
