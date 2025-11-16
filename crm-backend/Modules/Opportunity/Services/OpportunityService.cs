using crm_backend.Data;
using crm_backend.Modules.Opportunity.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Opportunity.Services;

public class OpportunityService : IOpportunityService
{
    private readonly CrmDbContext _context;

    public OpportunityService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OpportunityDto>> GetAllOpportunitiesAsync(int? companyId = null)
    {
        var query = _context.Opportunities
            .Include(o => o.Company)
            .Include(o => o.Customer)
            .Include(o => o.PipelineStage)
            .Include(o => o.AssignedUser)
            .Include(o => o.Source)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(o => o.CompanyId == companyId.Value);
        }

        var opportunities = await query
            .Select(o => new OpportunityDto
            {
                Id = o.Id,
                Name = o.Name,
                Description = o.Description,
                Amount = o.Amount,
                Currency = o.Currency,
                Probability = o.Probability,
                WeightedAmount = o.WeightedAmount,
                PipelineStageId = o.PipelineStageId,
                PipelineStageName = o.PipelineStage.Name,
                ExpectedCloseDate = o.ExpectedCloseDate,
                ActualCloseDate = o.ActualCloseDate,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer.Name,
                AssignedUserId = o.AssignedUserId,
                AssignedUserName = o.AssignedUser != null ? o.AssignedUser.Name : null,
                SourceId = o.SourceId,
                SourceName = o.Source != null ? o.Source.Name : null,
                Status = o.Status,
                LostReason = o.LostReason,
                WinReason = o.WinReason,
                CompanyId = o.CompanyId,
                CompanyName = o.Company.Name,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt,
                WonAt = o.WonAt,
                LostAt = o.LostAt
            })
            .ToListAsync();

        return opportunities;
    }

    public async Task<OpportunityDto?> GetOpportunityByIdAsync(int id)
    {
        var opportunity = await _context.Opportunities
            .Include(o => o.Company)
            .Include(o => o.Customer)
            .Include(o => o.PipelineStage)
            .Include(o => o.AssignedUser)
            .Include(o => o.Source)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (opportunity == null) return null;

        return new OpportunityDto
        {
            Id = opportunity.Id,
            Name = opportunity.Name,
            Description = opportunity.Description,
            Amount = opportunity.Amount,
            Currency = opportunity.Currency,
            Probability = opportunity.Probability,
            WeightedAmount = opportunity.WeightedAmount,
            PipelineStageId = opportunity.PipelineStageId,
            PipelineStageName = opportunity.PipelineStage.Name,
            ExpectedCloseDate = opportunity.ExpectedCloseDate,
            ActualCloseDate = opportunity.ActualCloseDate,
            CustomerId = opportunity.CustomerId,
            CustomerName = opportunity.Customer.Name,
            AssignedUserId = opportunity.AssignedUserId,
            AssignedUserName = opportunity.AssignedUser != null ? opportunity.AssignedUser.Name : null,
            SourceId = opportunity.SourceId,
            SourceName = opportunity.Source != null ? opportunity.Source.Name : null,
            Status = opportunity.Status,
            LostReason = opportunity.LostReason,
            WinReason = opportunity.WinReason,
            CompanyId = opportunity.CompanyId,
            CompanyName = opportunity.Company.Name,
            CreatedAt = opportunity.CreatedAt,
            UpdatedAt = opportunity.UpdatedAt,
            WonAt = opportunity.WonAt,
            LostAt = opportunity.LostAt
        };
    }

    public async Task<OpportunityDto> CreateOpportunityAsync(CreateOpportunityDto dto)
    {
        // Verify company exists
        var companyId = dto.CompanyId ?? throw new InvalidOperationException("Company ID is required");
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId);
        if (!companyExists)
        {
            throw new InvalidOperationException($"Company with ID {companyId} does not exist.");
        }

        // Verify customer exists and belongs to company
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == dto.CustomerId);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer with ID {dto.CustomerId} does not exist.");
        }
        if (customer.CompanyId != companyId)
        {
            throw new InvalidOperationException("Customer does not belong to the specified company.");
        }

        // Verify pipeline stage exists and belongs to company
        var pipelineStage = await _context.PipelineStages.FirstOrDefaultAsync(ps => ps.Id == dto.PipelineStageId);
        if (pipelineStage == null)
        {
            throw new InvalidOperationException($"Pipeline stage with ID {dto.PipelineStageId} does not exist.");
        }
        if (pipelineStage.CompanyId != companyId)
        {
            throw new InvalidOperationException("Pipeline stage does not belong to the specified company.");
        }

        // Verify assigned user exists and belongs to company (if provided)
        if (dto.AssignedUserId.HasValue)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.AssignedUserId.Value);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {dto.AssignedUserId.Value} does not exist.");
            }
            var userCompany = await _context.UserCompanies
                .AnyAsync(uc => uc.UserId == dto.AssignedUserId.Value && uc.CompanyId == companyId);
            if (!userCompany)
            {
                throw new InvalidOperationException("User does not belong to the specified company.");
            }
        }

        // Verify lead source exists and belongs to company (if provided)
        if (dto.SourceId.HasValue)
        {
            var source = await _context.LeadSources.FirstOrDefaultAsync(ls => ls.Id == dto.SourceId.Value);
            if (source == null)
            {
                throw new InvalidOperationException($"Lead source with ID {dto.SourceId.Value} does not exist.");
            }
            if (source.CompanyId != companyId)
            {
                throw new InvalidOperationException("Lead source does not belong to the specified company.");
            }
        }

        var opportunity = new Opportunity
        {
            Name = dto.Name,
            Description = dto.Description,
            Amount = dto.Amount,
            Currency = dto.Currency,
            Probability = dto.Probability,
            PipelineStageId = dto.PipelineStageId,
            ExpectedCloseDate = dto.ExpectedCloseDate,
            CustomerId = dto.CustomerId,
            AssignedUserId = dto.AssignedUserId,
            SourceId = dto.SourceId,
            CompanyId = companyId,
            Status = OpportunityStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Load related entities for response
        await _context.Entry(opportunity).Reference(o => o.Company).LoadAsync();
        await _context.Entry(opportunity).Reference(o => o.Customer).LoadAsync();
        await _context.Entry(opportunity).Reference(o => o.PipelineStage).LoadAsync();
        if (opportunity.AssignedUserId.HasValue)
        {
            await _context.Entry(opportunity).Reference(o => o.AssignedUser).LoadAsync();
        }
        if (opportunity.SourceId.HasValue)
        {
            await _context.Entry(opportunity).Reference(o => o.Source).LoadAsync();
        }

        return new OpportunityDto
        {
            Id = opportunity.Id,
            Name = opportunity.Name,
            Description = opportunity.Description,
            Amount = opportunity.Amount,
            Currency = opportunity.Currency,
            Probability = opportunity.Probability,
            WeightedAmount = opportunity.WeightedAmount,
            PipelineStageId = opportunity.PipelineStageId,
            PipelineStageName = opportunity.PipelineStage.Name,
            ExpectedCloseDate = opportunity.ExpectedCloseDate,
            ActualCloseDate = opportunity.ActualCloseDate,
            CustomerId = opportunity.CustomerId,
            CustomerName = opportunity.Customer.Name,
            AssignedUserId = opportunity.AssignedUserId,
            AssignedUserName = opportunity.AssignedUser != null ? opportunity.AssignedUser.Name : null,
            SourceId = opportunity.SourceId,
            SourceName = opportunity.Source != null ? opportunity.Source.Name : null,
            Status = opportunity.Status,
            LostReason = opportunity.LostReason,
            WinReason = opportunity.WinReason,
            CompanyId = opportunity.CompanyId,
            CompanyName = opportunity.Company.Name,
            CreatedAt = opportunity.CreatedAt,
            UpdatedAt = opportunity.UpdatedAt,
            WonAt = opportunity.WonAt,
            LostAt = opportunity.LostAt
        };
    }

    public async Task<OpportunityDto?> UpdateOpportunityAsync(int id, UpdateOpportunityDto dto)
    {
        var opportunity = await _context.Opportunities
            .Include(o => o.Company)
            .Include(o => o.Customer)
            .Include(o => o.PipelineStage)
            .Include(o => o.AssignedUser)
            .Include(o => o.Source)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (opportunity == null) return null;

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            opportunity.Name = dto.Name;
        }

        if (dto.Description != null)
        {
            opportunity.Description = dto.Description;
        }

        if (dto.Amount.HasValue)
        {
            opportunity.Amount = dto.Amount.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.Currency))
        {
            opportunity.Currency = dto.Currency;
        }

        if (dto.Probability.HasValue)
        {
            opportunity.Probability = dto.Probability.Value;
        }

        if (dto.PipelineStageId.HasValue)
        {
            // Verify pipeline stage exists and belongs to company
            var pipelineStage = await _context.PipelineStages
                .FirstOrDefaultAsync(ps => ps.Id == dto.PipelineStageId.Value);
            if (pipelineStage == null)
            {
                throw new InvalidOperationException($"Pipeline stage with ID {dto.PipelineStageId.Value} does not exist.");
            }
            if (pipelineStage.CompanyId != opportunity.CompanyId)
            {
                throw new InvalidOperationException("Pipeline stage does not belong to the opportunity's company.");
            }
            opportunity.PipelineStageId = dto.PipelineStageId.Value;
        }

        if (dto.ExpectedCloseDate.HasValue)
        {
            opportunity.ExpectedCloseDate = dto.ExpectedCloseDate;
        }

        if (dto.AssignedUserId.HasValue)
        {
            // Verify user exists and belongs to company
            var userCompany = await _context.UserCompanies
                .AnyAsync(uc => uc.UserId == dto.AssignedUserId.Value && uc.CompanyId == opportunity.CompanyId);
            if (!userCompany)
            {
                throw new InvalidOperationException("User does not belong to the opportunity's company.");
            }
            opportunity.AssignedUserId = dto.AssignedUserId;
        }

        if (dto.SourceId.HasValue)
        {
            // Verify lead source exists and belongs to company
            var source = await _context.LeadSources
                .FirstOrDefaultAsync(ls => ls.Id == dto.SourceId.Value);
            if (source == null)
            {
                throw new InvalidOperationException($"Lead source with ID {dto.SourceId.Value} does not exist.");
            }
            if (source.CompanyId != opportunity.CompanyId)
            {
                throw new InvalidOperationException("Lead source does not belong to the opportunity's company.");
            }
            opportunity.SourceId = dto.SourceId;
        }

        if (dto.Status.HasValue)
        {
            opportunity.Status = dto.Status.Value;

            // Update timestamps based on status
            if (dto.Status.Value == OpportunityStatus.Won)
            {
                opportunity.WonAt = DateTime.UtcNow;
                opportunity.ActualCloseDate = DateTime.UtcNow;
            }
            else if (dto.Status.Value == OpportunityStatus.Lost)
            {
                opportunity.LostAt = DateTime.UtcNow;
                opportunity.ActualCloseDate = DateTime.UtcNow;
            }
        }

        if (dto.LostReason != null)
        {
            opportunity.LostReason = dto.LostReason;
        }

        if (dto.WinReason != null)
        {
            opportunity.WinReason = dto.WinReason;
        }

        opportunity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Reload related entities
        await _context.Entry(opportunity).Reference(o => o.PipelineStage).LoadAsync();
        if (opportunity.AssignedUserId.HasValue)
        {
            await _context.Entry(opportunity).Reference(o => o.AssignedUser).LoadAsync();
        }
        if (opportunity.SourceId.HasValue)
        {
            await _context.Entry(opportunity).Reference(o => o.Source).LoadAsync();
        }

        return new OpportunityDto
        {
            Id = opportunity.Id,
            Name = opportunity.Name,
            Description = opportunity.Description,
            Amount = opportunity.Amount,
            Currency = opportunity.Currency,
            Probability = opportunity.Probability,
            WeightedAmount = opportunity.WeightedAmount,
            PipelineStageId = opportunity.PipelineStageId,
            PipelineStageName = opportunity.PipelineStage.Name,
            ExpectedCloseDate = opportunity.ExpectedCloseDate,
            ActualCloseDate = opportunity.ActualCloseDate,
            CustomerId = opportunity.CustomerId,
            CustomerName = opportunity.Customer.Name,
            AssignedUserId = opportunity.AssignedUserId,
            AssignedUserName = opportunity.AssignedUser != null ? opportunity.AssignedUser.Name : null,
            SourceId = opportunity.SourceId,
            SourceName = opportunity.Source != null ? opportunity.Source.Name : null,
            Status = opportunity.Status,
            LostReason = opportunity.LostReason,
            WinReason = opportunity.WinReason,
            CompanyId = opportunity.CompanyId,
            CompanyName = opportunity.Company.Name,
            CreatedAt = opportunity.CreatedAt,
            UpdatedAt = opportunity.UpdatedAt,
            WonAt = opportunity.WonAt,
            LostAt = opportunity.LostAt
        };
    }

    public async Task<bool> DeleteOpportunityAsync(int id)
    {
        var opportunity = await _context.Opportunities.FindAsync(id);
        if (opportunity == null) return false;

        _context.Opportunities.Remove(opportunity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<OpportunityDto?> MoveOpportunityToStageAsync(int id, int pipelineStageId)
    {
        var opportunity = await _context.Opportunities
            .Include(o => o.Company)
            .Include(o => o.Customer)
            .Include(o => o.PipelineStage)
            .Include(o => o.AssignedUser)
            .Include(o => o.Source)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (opportunity == null) return null;

        // Verify pipeline stage exists and belongs to company
        var pipelineStage = await _context.PipelineStages
            .FirstOrDefaultAsync(ps => ps.Id == pipelineStageId);
        if (pipelineStage == null)
        {
            throw new InvalidOperationException($"Pipeline stage with ID {pipelineStageId} does not exist.");
        }
        if (pipelineStage.CompanyId != opportunity.CompanyId)
        {
            throw new InvalidOperationException("Pipeline stage does not belong to the opportunity's company.");
        }

        opportunity.PipelineStageId = pipelineStageId;
        opportunity.Probability = pipelineStage.DefaultProbability;
        opportunity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Reload pipeline stage
        await _context.Entry(opportunity).Reference(o => o.PipelineStage).LoadAsync();

        return new OpportunityDto
        {
            Id = opportunity.Id,
            Name = opportunity.Name,
            Description = opportunity.Description,
            Amount = opportunity.Amount,
            Currency = opportunity.Currency,
            Probability = opportunity.Probability,
            WeightedAmount = opportunity.WeightedAmount,
            PipelineStageId = opportunity.PipelineStageId,
            PipelineStageName = opportunity.PipelineStage.Name,
            ExpectedCloseDate = opportunity.ExpectedCloseDate,
            ActualCloseDate = opportunity.ActualCloseDate,
            CustomerId = opportunity.CustomerId,
            CustomerName = opportunity.Customer.Name,
            AssignedUserId = opportunity.AssignedUserId,
            AssignedUserName = opportunity.AssignedUser != null ? opportunity.AssignedUser.Name : null,
            SourceId = opportunity.SourceId,
            SourceName = opportunity.Source != null ? opportunity.Source.Name : null,
            Status = opportunity.Status,
            LostReason = opportunity.LostReason,
            WinReason = opportunity.WinReason,
            CompanyId = opportunity.CompanyId,
            CompanyName = opportunity.Company.Name,
            CreatedAt = opportunity.CreatedAt,
            UpdatedAt = opportunity.UpdatedAt,
            WonAt = opportunity.WonAt,
            LostAt = opportunity.LostAt
        };
    }

    public async Task<decimal> CalculateWeightedPipelineValueAsync(int companyId)
    {
        var weightedValue = await _context.Opportunities
            .Where(o => o.CompanyId == companyId && o.Status == OpportunityStatus.Open)
            .SumAsync(o => o.WeightedAmount);

        return weightedValue;
    }
}

