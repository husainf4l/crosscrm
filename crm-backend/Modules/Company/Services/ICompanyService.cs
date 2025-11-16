using crm_backend.Modules.Company.DTOs;

namespace crm_backend.Modules.Company.Services;

public interface ICompanyService
{
    Task<IEnumerable<CompanyDto>> GetAllCompaniesAsync();
    Task<CompanyDto?> GetCompanyByIdAsync(int id);
    Task<CompanyDto> CreateCompanyAsync(CreateCompanyDto dto);
    Task<CompanyDto?> UpdateCompanyAsync(int id, UpdateCompanyDto dto);
    Task<bool> DeleteCompanyAsync(int id);
}
