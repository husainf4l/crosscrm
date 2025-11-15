using crm_backend.Data;
using crm_backend.Modules.Opportunity.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Opportunity.Services;

public class PipelineStageService : IPipelineStageService
{
    private readonly CrmDbContext _context;

    public PipelineStageService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PipelineStageDto>> GetAllPipelineStagesAsync(int? companyId = null)
    {
        var query = _context.PipelineStages
            .Include(ps => ps.Company)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(ps => ps.CompanyId == companyId.Value);
        }

        var stages = await query
            .OrderBy(ps => ps.Order)
            .Select(ps => new PipelineStageDto
            {
                Id = ps.Id,
                Name = ps.Name,
                Description = ps.Description,
                Order = ps.Order,
                DefaultProbability = ps.DefaultProbability,
                IsActive = ps.IsActive,
                IsWonStage = ps.IsWonStage,
                IsLostStage = ps.IsLostStage,
                CompanyId = ps.CompanyId,
                CompanyName = ps.Company.Name
            })
            .ToListAsync();

        return stages;
    }

    public async Task<PipelineStageDto?> GetPipelineStageByIdAsync(int id)
    {
        var stage = await _context.PipelineStages
            .Include(ps => ps.Company)
            .FirstOrDefaultAsync(ps => ps.Id == id);

        if (stage == null) return null;

        return new PipelineStageDto
        {
            Id = stage.Id,
            Name = stage.Name,
            Description = stage.Description,
            Order = stage.Order,
            DefaultProbability = stage.DefaultProbability,
            IsActive = stage.IsActive,
            IsWonStage = stage.IsWonStage,
            IsLostStage = stage.IsLostStage,
            CompanyId = stage.CompanyId,
            CompanyName = stage.Company.Name
        };
    }

    public async Task<PipelineStageDto> CreatePipelineStageAsync(CreatePipelineStageDto dto)
    {
        var companyId = dto.CompanyId ?? throw new InvalidOperationException("Company ID is required");
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId);
        if (!companyExists)
        {
            throw new InvalidOperationException($"Company with ID {companyId} does not exist.");
        }

        var stage = new PipelineStage
        {
            Name = dto.Name,
            Description = dto.Description,
            Order = dto.Order,
            DefaultProbability = dto.DefaultProbability,
            IsActive = dto.IsActive,
            IsWonStage = dto.IsWonStage,
            IsLostStage = dto.IsLostStage,
            CompanyId = companyId
        };

        _context.PipelineStages.Add(stage);
        await _context.SaveChangesAsync();

        await _context.Entry(stage).Reference(ps => ps.Company).LoadAsync();

        return new PipelineStageDto
        {
            Id = stage.Id,
            Name = stage.Name,
            Description = stage.Description,
            Order = stage.Order,
            DefaultProbability = stage.DefaultProbability,
            IsActive = stage.IsActive,
            IsWonStage = stage.IsWonStage,
            IsLostStage = stage.IsLostStage,
            CompanyId = stage.CompanyId,
            CompanyName = stage.Company.Name
        };
    }

    public async Task<PipelineStageDto?> UpdatePipelineStageAsync(int id, UpdatePipelineStageDto dto)
    {
        var stage = await _context.PipelineStages
            .Include(ps => ps.Company)
            .FirstOrDefaultAsync(ps => ps.Id == id);

        if (stage == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            stage.Name = dto.Name;
        }

        if (dto.Description != null)
        {
            stage.Description = dto.Description;
        }

        if (dto.Order.HasValue)
        {
            stage.Order = dto.Order.Value;
        }

        if (dto.DefaultProbability.HasValue)
        {
            stage.DefaultProbability = dto.DefaultProbability.Value;
        }

        if (dto.IsActive.HasValue)
        {
            stage.IsActive = dto.IsActive.Value;
        }

        if (dto.IsWonStage.HasValue)
        {
            stage.IsWonStage = dto.IsWonStage.Value;
        }

        if (dto.IsLostStage.HasValue)
        {
            stage.IsLostStage = dto.IsLostStage.Value;
        }

        await _context.SaveChangesAsync();

        return new PipelineStageDto
        {
            Id = stage.Id,
            Name = stage.Name,
            Description = stage.Description,
            Order = stage.Order,
            DefaultProbability = stage.DefaultProbability,
            IsActive = stage.IsActive,
            IsWonStage = stage.IsWonStage,
            IsLostStage = stage.IsLostStage,
            CompanyId = stage.CompanyId,
            CompanyName = stage.Company.Name
        };
    }

    public async Task<bool> DeletePipelineStageAsync(int id)
    {
        var stage = await _context.PipelineStages.FindAsync(id);
        if (stage == null) return false;

        // Check if any opportunities are using this stage
        var hasOpportunities = await _context.Opportunities
            .AnyAsync(o => o.PipelineStageId == id);
        
        if (hasOpportunities)
        {
            throw new InvalidOperationException("Cannot delete pipeline stage that has associated opportunities.");
        }

        _context.PipelineStages.Remove(stage);
        await _context.SaveChangesAsync();
        return true;
    }
}

