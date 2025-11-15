using System.Text.Json;
using crm_backend.Data;
using crm_backend.Modules.Collaboration;
using crm_backend.Modules.Collaboration.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Collaboration.Services;

public class CustomerWorkspaceService : ICustomerWorkspaceService
{
    private readonly CrmDbContext _context;

    public CustomerWorkspaceService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerWorkspaceDto?> GetWorkspaceByCustomerIdAsync(int customerId, int companyId)
    {
        var workspace = await _context.CustomerWorkspaces
            .Include(cw => cw.Customer)
            .Include(cw => cw.Team)
            .Include(cw => cw.LastUpdatedByUser)
            .FirstOrDefaultAsync(cw => cw.CustomerId == customerId && cw.CompanyId == companyId);

        if (workspace == null) return null;

        var strategyCount = await _context.CustomerStrategies.CountAsync(cs => cs.WorkspaceId == workspace.Id);
        var ideaCount = await _context.CustomerIdeas.CountAsync(ci => ci.WorkspaceId == workspace.Id);

        return new CustomerWorkspaceDto
        {
            Id = workspace.Id,
            CustomerId = workspace.CustomerId,
            CustomerName = workspace.Customer.Name,
            TeamId = workspace.TeamId,
            TeamName = workspace.Team?.Name,
            Summary = workspace.Summary,
            LastUpdatedAt = workspace.LastUpdatedAt,
            LastUpdatedByUserId = workspace.LastUpdatedByUserId,
            LastUpdatedByUserName = workspace.LastUpdatedByUser?.Name,
            CompanyId = workspace.CompanyId,
            CreatedAt = workspace.CreatedAt,
            UpdatedAt = workspace.UpdatedAt,
            StrategyCount = strategyCount,
            IdeaCount = ideaCount
        };
    }

    public async Task<CustomerWorkspaceDto> GetOrCreateWorkspaceAsync(int customerId, int companyId)
    {
        var existing = await GetWorkspaceByCustomerIdAsync(customerId, companyId);
        if (existing != null)
            return existing;

        var dto = new CreateCustomerWorkspaceDto { CustomerId = customerId };
        return await CreateWorkspaceAsync(dto, companyId);
    }

    public async Task<CustomerWorkspaceDto> CreateWorkspaceAsync(CreateCustomerWorkspaceDto dto, int companyId)
    {
        // Verify customer exists and belongs to company
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == dto.CustomerId && c.CompanyId == companyId);
        if (customer == null)
        {
            throw new InvalidOperationException("Customer not found or does not belong to the company.");
        }

        // Verify team exists and belongs to company (if provided)
        if (dto.TeamId.HasValue)
        {
            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.Id == dto.TeamId.Value && t.CompanyId == companyId);
            if (team == null)
            {
                throw new InvalidOperationException("Team not found or does not belong to the company.");
            }
        }

        // Check if workspace already exists
        var existing = await _context.CustomerWorkspaces
            .FirstOrDefaultAsync(cw => cw.CustomerId == dto.CustomerId && cw.CompanyId == companyId);
        if (existing != null)
        {
            throw new InvalidOperationException("Workspace already exists for this customer.");
        }

        var workspace = new CustomerWorkspace
        {
            CustomerId = dto.CustomerId,
            TeamId = dto.TeamId,
            Summary = dto.Summary,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.CustomerWorkspaces.Add(workspace);
        await _context.SaveChangesAsync();

        return await GetWorkspaceByCustomerIdAsync(workspace.CustomerId, companyId) 
            ?? throw new InvalidOperationException("Failed to retrieve created workspace");
    }

    public async Task<CustomerWorkspaceDto?> UpdateWorkspaceAsync(int id, UpdateCustomerWorkspaceDto dto, int companyId, int? updatedByUserId = null)
    {
        var workspace = await _context.CustomerWorkspaces
            .FirstOrDefaultAsync(cw => cw.Id == id && cw.CompanyId == companyId);
        if (workspace == null) return null;

        if (dto.TeamId.HasValue)
        {
            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.Id == dto.TeamId.Value && t.CompanyId == companyId);
            if (team == null)
            {
                throw new InvalidOperationException("Team not found or does not belong to the company.");
            }
            workspace.TeamId = dto.TeamId;
        }

        if (dto.Summary != null)
            workspace.Summary = dto.Summary;

        workspace.UpdatedAt = DateTime.UtcNow;
        workspace.LastUpdatedAt = DateTime.UtcNow;
        if (updatedByUserId.HasValue)
            workspace.LastUpdatedByUserId = updatedByUserId;

        await _context.SaveChangesAsync();

        return await GetWorkspaceByCustomerIdAsync(workspace.CustomerId, companyId);
    }

    public async Task<bool> DeleteWorkspaceAsync(int id, int companyId)
    {
        var workspace = await _context.CustomerWorkspaces
            .FirstOrDefaultAsync(cw => cw.Id == id && cw.CompanyId == companyId);
        if (workspace == null) return false;

        _context.CustomerWorkspaces.Remove(workspace);
        await _context.SaveChangesAsync();

        return true;
    }
}

