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
                CreatedAt = c.CreatedAt,
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
            CreatedAt = company.CreatedAt,
            UserCount = company.UserCompanies.Count
        };
    }

    public async Task<CompanyDto> CreateCompanyAsync(CreateCompanyDto dto)
    {
        var company = new Company
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Description = company.Description,
            CreatedAt = company.CreatedAt,
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

        await _context.SaveChangesAsync();

        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Description = company.Description,
            CreatedAt = company.CreatedAt,
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
