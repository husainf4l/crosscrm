using crm_backend.Data;
using crm_backend.Modules.Opportunity.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Opportunity.Services;

public class LeadSourceService : ILeadSourceService
{
    private readonly CrmDbContext _context;

    public LeadSourceService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LeadSourceDto>> GetAllLeadSourcesAsync(int? companyId = null)
    {
        var query = _context.LeadSources
            .Include(ls => ls.Company)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(ls => ls.CompanyId == companyId.Value);
        }

        var sources = await query
            .Select(ls => new LeadSourceDto
            {
                Id = ls.Id,
                Name = ls.Name,
                Description = ls.Description,
                IsActive = ls.IsActive,
                CompanyId = ls.CompanyId,
                CompanyName = ls.Company.Name
            })
            .ToListAsync();

        return sources;
    }

    public async Task<LeadSourceDto?> GetLeadSourceByIdAsync(int id)
    {
        var source = await _context.LeadSources
            .Include(ls => ls.Company)
            .FirstOrDefaultAsync(ls => ls.Id == id);

        if (source == null) return null;

        return new LeadSourceDto
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            IsActive = source.IsActive,
            CompanyId = source.CompanyId,
            CompanyName = source.Company.Name
        };
    }

    public async Task<LeadSourceDto> CreateLeadSourceAsync(CreateLeadSourceDto dto)
    {
        var companyId = dto.CompanyId ?? throw new InvalidOperationException("Company ID is required");
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId);
        if (!companyExists)
        {
            throw new InvalidOperationException($"Company with ID {companyId} does not exist.");
        }

        var source = new LeadSource
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive,
            CompanyId = companyId
        };

        _context.LeadSources.Add(source);
        await _context.SaveChangesAsync();

        await _context.Entry(source).Reference(ls => ls.Company).LoadAsync();

        return new LeadSourceDto
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            IsActive = source.IsActive,
            CompanyId = source.CompanyId,
            CompanyName = source.Company.Name
        };
    }

    public async Task<LeadSourceDto?> UpdateLeadSourceAsync(int id, UpdateLeadSourceDto dto)
    {
        var source = await _context.LeadSources
            .Include(ls => ls.Company)
            .FirstOrDefaultAsync(ls => ls.Id == id);

        if (source == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            source.Name = dto.Name;
        }

        if (dto.Description != null)
        {
            source.Description = dto.Description;
        }

        if (dto.IsActive.HasValue)
        {
            source.IsActive = dto.IsActive.Value;
        }

        await _context.SaveChangesAsync();

        return new LeadSourceDto
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            IsActive = source.IsActive,
            CompanyId = source.CompanyId,
            CompanyName = source.Company.Name
        };
    }

    public async Task<bool> DeleteLeadSourceAsync(int id)
    {
        var source = await _context.LeadSources.FindAsync(id);
        if (source == null) return false;

        // Check if any opportunities are using this source
        var hasOpportunities = await _context.Opportunities
            .AnyAsync(o => o.SourceId == id);
        
        if (hasOpportunities)
        {
            throw new InvalidOperationException("Cannot delete lead source that has associated opportunities.");
        }

        _context.LeadSources.Remove(source);
        await _context.SaveChangesAsync();
        return true;
    }
}

