using System.Text.Json;
using crm_backend.Data;
using crm_backend.Modules.Collaboration.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Collaboration.Services;

public class CustomerStrategyService : ICustomerStrategyService
{
    private readonly CrmDbContext _context;

    public CustomerStrategyService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomerStrategyDto>> GetStrategiesByCustomerAsync(int customerId, int companyId)
    {
        var strategies = await _context.CustomerStrategies
            .Include(cs => cs.Customer)
            .Include(cs => cs.CreatedByUser)
            .Include(cs => cs.AssignedTeam)
            .Where(cs => cs.CustomerId == customerId && cs.CompanyId == companyId)
            .Select(cs => new CustomerStrategyDto
            {
                Id = cs.Id,
                Title = cs.Title,
                Description = cs.Description,
                Type = cs.Type.ToString(),
                Status = cs.Status.ToString(),
                Priority = cs.Priority.ToString(),
                StartDate = cs.StartDate,
                EndDate = cs.EndDate,
                SuccessMetrics = !string.IsNullOrEmpty(cs.SuccessMetrics)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(cs.SuccessMetrics)
                    : null,
                CustomerId = cs.CustomerId,
                CustomerName = cs.Customer.Name,
                WorkspaceId = cs.WorkspaceId,
                CreatedByUserId = cs.CreatedByUserId,
                CreatedByUserName = cs.CreatedByUser.Name,
                AssignedToTeamId = cs.AssignedToTeamId,
                AssignedTeamName = cs.AssignedTeam != null ? cs.AssignedTeam.Name : null,
                CompanyId = cs.CompanyId,
                CreatedAt = cs.CreatedAt,
                UpdatedAt = cs.UpdatedAt,
                CommentCount = _context.NoteComments.Count(nc => nc.EntityType == "Strategy" && nc.EntityId == cs.Id)
            })
            .ToListAsync();

        return strategies;
    }

    public async Task<IEnumerable<CustomerStrategyDto>> GetStrategiesByWorkspaceAsync(int workspaceId, int companyId)
    {
        var strategies = await _context.CustomerStrategies
            .Include(cs => cs.Customer)
            .Include(cs => cs.CreatedByUser)
            .Include(cs => cs.AssignedTeam)
            .Where(cs => cs.WorkspaceId == workspaceId && cs.CompanyId == companyId)
            .Select(cs => new CustomerStrategyDto
            {
                Id = cs.Id,
                Title = cs.Title,
                Description = cs.Description,
                Type = cs.Type.ToString(),
                Status = cs.Status.ToString(),
                Priority = cs.Priority.ToString(),
                StartDate = cs.StartDate,
                EndDate = cs.EndDate,
                SuccessMetrics = !string.IsNullOrEmpty(cs.SuccessMetrics)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(cs.SuccessMetrics)
                    : null,
                CustomerId = cs.CustomerId,
                CustomerName = cs.Customer.Name,
                WorkspaceId = cs.WorkspaceId,
                CreatedByUserId = cs.CreatedByUserId,
                CreatedByUserName = cs.CreatedByUser.Name,
                AssignedToTeamId = cs.AssignedToTeamId,
                AssignedTeamName = cs.AssignedTeam != null ? cs.AssignedTeam.Name : null,
                CompanyId = cs.CompanyId,
                CreatedAt = cs.CreatedAt,
                UpdatedAt = cs.UpdatedAt,
                CommentCount = _context.NoteComments.Count(nc => nc.EntityType == "Strategy" && nc.EntityId == cs.Id)
            })
            .ToListAsync();

        return strategies;
    }

    public async Task<CustomerStrategyDto?> GetStrategyByIdAsync(int id)
    {
        var strategy = await _context.CustomerStrategies
            .Include(cs => cs.Customer)
            .Include(cs => cs.CreatedByUser)
            .Include(cs => cs.AssignedTeam)
            .FirstOrDefaultAsync(cs => cs.Id == id);

        if (strategy == null) return null;

        var commentCount = await _context.NoteComments
            .CountAsync(nc => nc.EntityType == "Strategy" && nc.EntityId == strategy.Id);

        return new CustomerStrategyDto
        {
            Id = strategy.Id,
            Title = strategy.Title,
            Description = strategy.Description,
            Type = strategy.Type.ToString(),
            Status = strategy.Status.ToString(),
            Priority = strategy.Priority.ToString(),
            StartDate = strategy.StartDate,
            EndDate = strategy.EndDate,
            SuccessMetrics = !string.IsNullOrEmpty(strategy.SuccessMetrics)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(strategy.SuccessMetrics)
                : null,
            CustomerId = strategy.CustomerId,
            CustomerName = strategy.Customer.Name,
            WorkspaceId = strategy.WorkspaceId,
            CreatedByUserId = strategy.CreatedByUserId,
            CreatedByUserName = strategy.CreatedByUser.Name,
            AssignedToTeamId = strategy.AssignedToTeamId,
            AssignedTeamName = strategy.AssignedTeam != null ? strategy.AssignedTeam.Name : null,
            CompanyId = strategy.CompanyId,
            CreatedAt = strategy.CreatedAt,
            UpdatedAt = strategy.UpdatedAt,
            CommentCount = commentCount
        };
    }

    public async Task<CustomerStrategyDto> CreateStrategyAsync(CreateCustomerStrategyDto dto, int companyId, int createdByUserId)
    {
        // Verify customer exists and belongs to company
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == dto.CustomerId && c.CompanyId == companyId);
        if (customer == null)
        {
            throw new InvalidOperationException("Customer not found or does not belong to the company.");
        }

        // Verify workspace exists and belongs to company (if provided)
        if (dto.WorkspaceId.HasValue)
        {
            var workspace = await _context.CustomerWorkspaces
                .FirstOrDefaultAsync(cw => cw.Id == dto.WorkspaceId.Value && cw.CompanyId == companyId);
            if (workspace == null)
            {
                throw new InvalidOperationException("Workspace not found or does not belong to the company.");
            }
        }

        // Verify team exists and belongs to company (if provided)
        if (dto.AssignedToTeamId.HasValue)
        {
            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.Id == dto.AssignedToTeamId.Value && t.CompanyId == companyId);
            if (team == null)
            {
                throw new InvalidOperationException("Team not found or does not belong to the company.");
            }
        }

        // Parse enums
        if (!Enum.TryParse<StrategyType>(dto.Type, true, out var strategyType))
        {
            throw new ArgumentException($"Invalid strategy type: {dto.Type}");
        }

        if (!Enum.TryParse<StrategyPriority>(dto.Priority, true, out var priority))
        {
            throw new ArgumentException($"Invalid priority: {dto.Priority}");
        }

        var strategy = new CustomerStrategy
        {
            CustomerId = dto.CustomerId,
            WorkspaceId = dto.WorkspaceId,
            Title = dto.Title,
            Description = dto.Description,
            Type = strategyType,
            Status = StrategyStatus.Draft,
            Priority = priority,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            SuccessMetrics = dto.SuccessMetrics != null ? JsonSerializer.Serialize(dto.SuccessMetrics) : null,
            CreatedByUserId = createdByUserId,
            AssignedToTeamId = dto.AssignedToTeamId,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.CustomerStrategies.Add(strategy);
        await _context.SaveChangesAsync();

        return await GetStrategyByIdAsync(strategy.Id)
            ?? throw new InvalidOperationException("Failed to retrieve created strategy");
    }

    public async Task<CustomerStrategyDto?> UpdateStrategyAsync(int id, UpdateCustomerStrategyDto dto, int companyId)
    {
        var strategy = await _context.CustomerStrategies
            .FirstOrDefaultAsync(cs => cs.Id == id && cs.CompanyId == companyId);
        if (strategy == null) return null;

        if (!string.IsNullOrEmpty(dto.Title))
            strategy.Title = dto.Title;

        if (dto.Description != null)
            strategy.Description = dto.Description;

        if (!string.IsNullOrEmpty(dto.Type) && Enum.TryParse<StrategyType>(dto.Type, true, out var strategyType))
            strategy.Type = strategyType;

        if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<StrategyStatus>(dto.Status, true, out var status))
            strategy.Status = status;

        if (!string.IsNullOrEmpty(dto.Priority) && Enum.TryParse<StrategyPriority>(dto.Priority, true, out var priority))
            strategy.Priority = priority;

        if (dto.StartDate.HasValue)
            strategy.StartDate = dto.StartDate;

        if (dto.EndDate.HasValue)
            strategy.EndDate = dto.EndDate;

        if (dto.SuccessMetrics != null)
            strategy.SuccessMetrics = JsonSerializer.Serialize(dto.SuccessMetrics);

        if (dto.AssignedToTeamId.HasValue)
        {
            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.Id == dto.AssignedToTeamId.Value && t.CompanyId == companyId);
            if (team == null)
            {
                throw new InvalidOperationException("Team not found or does not belong to the company.");
            }
            strategy.AssignedToTeamId = dto.AssignedToTeamId;
        }

        strategy.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetStrategyByIdAsync(strategy.Id);
    }

    public async Task<bool> DeleteStrategyAsync(int id, int companyId)
    {
        var strategy = await _context.CustomerStrategies
            .FirstOrDefaultAsync(cs => cs.Id == id && cs.CompanyId == companyId);
        if (strategy == null) return false;

        _context.CustomerStrategies.Remove(strategy);
        await _context.SaveChangesAsync();

        return true;
    }
}

