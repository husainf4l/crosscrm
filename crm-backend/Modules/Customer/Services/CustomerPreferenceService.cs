using crm_backend.Data;
using crm_backend.Modules.Customer.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Customer.Services;

public class CustomerPreferenceService : ICustomerPreferenceService
{
    private readonly CrmDbContext _context;

    public CustomerPreferenceService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerPreferenceDto> CreatePreferenceAsync(CreateCustomerPreferenceDto dto, int userId)
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

        // Check if preference already exists
        var existingPreference = await _context.CustomerPreferences
            .FirstOrDefaultAsync(cp => cp.CustomerId == dto.CustomerId && cp.PreferenceKey == dto.PreferenceKey);

        if (existingPreference != null)
            throw new Exception("Preference already exists for this customer");

        var preference = new CustomerPreference
        {
            CustomerId = dto.CustomerId,
            PreferenceKey = dto.PreferenceKey,
            PreferenceValue = dto.PreferenceValue,
            Category = dto.Category,
            SetByUserId = userId
        };

        _context.CustomerPreferences.Add(preference);
        await _context.SaveChangesAsync();

        return await GetPreferenceDtoAsync(preference.Id);
    }

    public async Task<CustomerPreferenceDto?> UpdatePreferenceAsync(int preferenceId, UpdateCustomerPreferenceDto dto, int userId)
    {
        var preference = await _context.CustomerPreferences
            .Include(cp => cp.Customer)
            .FirstOrDefaultAsync(cp => cp.Id == preferenceId);

        if (preference == null)
            return null;

        // Verify access
        var userCompany = await _context.UserCompanies
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompanyId == preference.Customer.CompanyId && uc.IsActive);

        if (userCompany == null)
            throw new Exception("Access denied");

        if (dto.PreferenceValue != null)
            preference.PreferenceValue = dto.PreferenceValue;

        if (dto.IsActive.HasValue)
            preference.IsActive = dto.IsActive.Value;

        preference.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetPreferenceDtoAsync(preferenceId);
    }

    public async Task<bool> DeletePreferenceAsync(int preferenceId, int userId)
    {
        var preference = await _context.CustomerPreferences
            .Include(cp => cp.Customer)
            .FirstOrDefaultAsync(cp => cp.Id == preferenceId);

        if (preference == null)
            return false;

        // Verify access
        var userCompany = await _context.UserCompanies
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompanyId == preference.Customer.CompanyId && uc.IsActive);

        if (userCompany == null)
            throw new Exception("Access denied");

        _context.CustomerPreferences.Remove(preference);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<CustomerPreferenceDto>> GetCustomerPreferencesAsync(int customerId, int userId)
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

        var preferences = await _context.CustomerPreferences
            .Where(cp => cp.CustomerId == customerId && cp.IsActive)
            .Include(cp => cp.SetByUser)
            .ToListAsync();

        return preferences.Select(cp => new CustomerPreferenceDto
        {
            Id = cp.Id,
            CustomerId = cp.CustomerId,
            PreferenceKey = cp.PreferenceKey,
            PreferenceValue = cp.PreferenceValue,
            Category = cp.Category,
            IsActive = cp.IsActive,
            CreatedAt = cp.CreatedAt,
            UpdatedAt = cp.UpdatedAt,
            SetByUserId = cp.SetByUserId,
            SetByUserName = cp.SetByUser?.Name ?? "Unknown"
        });
    }

    public async Task<CustomerPreferenceDto?> GetPreferenceAsync(int preferenceId, int userId)
    {
        var preference = await _context.CustomerPreferences
            .Include(cp => cp.Customer)
            .Include(cp => cp.SetByUser)
            .FirstOrDefaultAsync(cp => cp.Id == preferenceId);

        if (preference == null)
            return null;

        // Verify access
        var userCompany = await _context.UserCompanies
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompanyId == preference.Customer.CompanyId && uc.IsActive);

        if (userCompany == null)
            throw new Exception("Access denied");

        return new CustomerPreferenceDto
        {
            Id = preference.Id,
            CustomerId = preference.CustomerId,
            PreferenceKey = preference.PreferenceKey,
            PreferenceValue = preference.PreferenceValue,
            Category = preference.Category,
            IsActive = preference.IsActive,
            CreatedAt = preference.CreatedAt,
            UpdatedAt = preference.UpdatedAt,
            SetByUserId = preference.SetByUserId,
            SetByUserName = preference.SetByUser?.Name ?? "Unknown"
        };
    }

    public async Task<CustomerPreferenceDto?> GetCustomerPreferenceByKeyAsync(int customerId, string key, int userId)
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

        var preference = await _context.CustomerPreferences
            .Include(cp => cp.SetByUser)
            .FirstOrDefaultAsync(cp => cp.CustomerId == customerId && cp.PreferenceKey == key && cp.IsActive);

        if (preference == null)
            return null;

        return new CustomerPreferenceDto
        {
            Id = preference.Id,
            CustomerId = preference.CustomerId,
            PreferenceKey = preference.PreferenceKey,
            PreferenceValue = preference.PreferenceValue,
            Category = preference.Category,
            IsActive = preference.IsActive,
            CreatedAt = preference.CreatedAt,
            UpdatedAt = preference.UpdatedAt,
            SetByUserId = preference.SetByUserId,
            SetByUserName = preference.SetByUser?.Name ?? "Unknown"
        };
    }

    private async Task<CustomerPreferenceDto> GetPreferenceDtoAsync(int id)
    {
        var preference = await _context.CustomerPreferences
            .Include(cp => cp.SetByUser)
            .FirstOrDefaultAsync(cp => cp.Id == id);

        if (preference == null)
            throw new Exception("Preference not found");

        return new CustomerPreferenceDto
        {
            Id = preference.Id,
            CustomerId = preference.CustomerId,
            PreferenceKey = preference.PreferenceKey,
            PreferenceValue = preference.PreferenceValue,
            Category = preference.Category,
            IsActive = preference.IsActive,
            CreatedAt = preference.CreatedAt,
            UpdatedAt = preference.UpdatedAt,
            SetByUserId = preference.SetByUserId,
            SetByUserName = preference.SetByUser?.Name ?? "Unknown"
        };
    }
}