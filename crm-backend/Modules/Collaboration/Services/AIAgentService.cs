using System.Text.Json;
using crm_backend.Data;
using crm_backend.Modules.Collaboration.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Collaboration.Services;

public class AIAgentService : IAIAgentService
{
    private readonly CrmDbContext _context;

    public AIAgentService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AIAgentDto>> GetAllAgentsAsync(int? companyId = null)
    {
        var query = _context.AIAgents
            .Include(a => a.Company)
            .Include(a => a.CreatedByUser)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(a => a.CompanyId == companyId.Value);
        }

        var agents = await query
            .Select(a => new AIAgentDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                Type = a.Type.ToString(),
                Status = a.Status.ToString(),
                SystemPrompt = a.SystemPrompt,
                Tools = JsonSerializer.Deserialize<List<string>>(a.Tools) ?? new List<string>(),
                PythonServiceUrl = a.PythonServiceUrl,
                AgentId = a.AgentId,
                CreatedByUserId = a.CreatedByUserId,
                CreatedByUserName = a.CreatedByUser.Name,
                CompanyId = a.CompanyId,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();

        return agents;
    }

    public async Task<AIAgentDto?> GetAgentByIdAsync(int id)
    {
        var agent = await _context.AIAgents
            .Include(a => a.Company)
            .Include(a => a.CreatedByUser)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (agent == null) return null;

        return new AIAgentDto
        {
            Id = agent.Id,
            Name = agent.Name,
            Description = agent.Description,
            Type = agent.Type.ToString(),
            Status = agent.Status.ToString(),
            SystemPrompt = agent.SystemPrompt,
            Tools = JsonSerializer.Deserialize<List<string>>(agent.Tools) ?? new List<string>(),
            PythonServiceUrl = agent.PythonServiceUrl,
            AgentId = agent.AgentId,
            CreatedByUserId = agent.CreatedByUserId,
            CreatedByUserName = agent.CreatedByUser.Name,
            CompanyId = agent.CompanyId,
            CreatedAt = agent.CreatedAt,
            UpdatedAt = agent.UpdatedAt
        };
    }

    public async Task<AIAgentDto> CreateAgentAsync(CreateAIAgentDto dto, int companyId, int createdByUserId)
    {
        // Verify company exists
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId);
        if (!companyExists)
        {
            throw new InvalidOperationException($"Company with ID {companyId} does not exist.");
        }

        // Verify user exists and belongs to company
        var userCompany = await _context.UserCompanies
            .AnyAsync(uc => uc.UserId == createdByUserId && uc.CompanyId == companyId);
        if (!userCompany)
        {
            throw new InvalidOperationException("User does not belong to the specified company.");
        }

        // Parse agent type
        if (!Enum.TryParse<AgentType>(dto.Type, true, out var agentType))
        {
            throw new ArgumentException($"Invalid agent type: {dto.Type}");
        }

        // Generate unique AgentId (UUID format)
        var agentId = Guid.NewGuid().ToString();

        var agent = new AIAgent
        {
            Name = dto.Name,
            Description = dto.Description,
            Type = agentType,
            Status = AgentStatus.Active,
            SystemPrompt = dto.SystemPrompt,
            Tools = JsonSerializer.Serialize(dto.Tools),
            PythonServiceUrl = dto.PythonServiceUrl ?? "http://localhost:8003",
            AgentId = agentId,
            CompanyId = companyId,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.AIAgents.Add(agent);
        await _context.SaveChangesAsync();

        return await GetAgentByIdAsync(agent.Id) ?? throw new InvalidOperationException("Failed to retrieve created agent");
    }

    public async Task<AIAgentDto?> UpdateAgentAsync(int id, UpdateAIAgentDto dto)
    {
        var agent = await _context.AIAgents.FindAsync(id);
        if (agent == null) return null;

        if (!string.IsNullOrEmpty(dto.Name))
            agent.Name = dto.Name;

        if (dto.Description != null)
            agent.Description = dto.Description;

        if (!string.IsNullOrEmpty(dto.Type) && Enum.TryParse<AgentType>(dto.Type, true, out var agentType))
            agent.Type = agentType;

        if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<AgentStatus>(dto.Status, true, out var status))
            agent.Status = status;

        if (!string.IsNullOrEmpty(dto.SystemPrompt))
            agent.SystemPrompt = dto.SystemPrompt;

        if (dto.Tools != null)
            agent.Tools = JsonSerializer.Serialize(dto.Tools);

        if (!string.IsNullOrEmpty(dto.PythonServiceUrl))
            agent.PythonServiceUrl = dto.PythonServiceUrl;

        agent.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetAgentByIdAsync(agent.Id);
    }

    public async Task<bool> DeleteAgentAsync(int id)
    {
        var agent = await _context.AIAgents.FindAsync(id);
        if (agent == null) return false;

        _context.AIAgents.Remove(agent);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<AIAgentAssignmentDto> AssignAgentAsync(CreateAIAgentAssignmentDto dto, int companyId, int assignedByUserId)
    {
        // Verify agent exists and belongs to company
        var agent = await _context.AIAgents
            .FirstOrDefaultAsync(a => a.Id == dto.AgentId && a.CompanyId == companyId);
        if (agent == null)
        {
            throw new InvalidOperationException("Agent not found or does not belong to the company.");
        }

        // Verify user exists and belongs to company
        var userCompany = await _context.UserCompanies
            .AnyAsync(uc => uc.UserId == assignedByUserId && uc.CompanyId == companyId);
        if (!userCompany)
        {
            throw new InvalidOperationException("User does not belong to the specified company.");
        }

        // Check if assignment already exists
        var existingAssignment = await _context.AIAgentAssignments
            .FirstOrDefaultAsync(aa => aa.AgentId == dto.AgentId
                && aa.EntityType == dto.EntityType
                && aa.EntityId == dto.EntityId
                && aa.CompanyId == companyId
                && aa.IsActive);

        if (existingAssignment != null)
        {
            throw new InvalidOperationException("Agent is already assigned to this entity.");
        }

        var assignment = new AIAgentAssignment
        {
            AgentId = dto.AgentId,
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            AssignedByUserId = assignedByUserId,
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            Notes = dto.Notes,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.AIAgentAssignments.Add(assignment);
        await _context.SaveChangesAsync();

        return await GetAssignmentByIdAsync(assignment.Id) ?? throw new InvalidOperationException("Failed to retrieve created assignment");
    }

    public async Task<bool> UnassignAgentAsync(int assignmentId)
    {
        var assignment = await _context.AIAgentAssignments.FindAsync(assignmentId);
        if (assignment == null) return false;

        assignment.IsActive = false;
        assignment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<AIAgentAssignmentDto>> GetAgentAssignmentsAsync(int agentId)
    {
        var assignments = await _context.AIAgentAssignments
            .Include(aa => aa.Agent)
            .Include(aa => aa.AssignedByUser)
            .Where(aa => aa.AgentId == agentId)
            .Select(aa => new AIAgentAssignmentDto
            {
                Id = aa.Id,
                AgentId = aa.AgentId,
                AgentName = aa.Agent.Name,
                EntityType = aa.EntityType,
                EntityId = aa.EntityId,
                AssignedByUserId = aa.AssignedByUserId,
                AssignedByUserName = aa.AssignedByUser.Name,
                AssignedAt = aa.AssignedAt,
                IsActive = aa.IsActive,
                Notes = aa.Notes,
                CreatedAt = aa.CreatedAt
            })
            .ToListAsync();

        return assignments;
    }

    public async Task<IEnumerable<AIAgentAssignmentDto>> GetAssignmentsByEntityAsync(string entityType, int entityId, int companyId)
    {
        var assignments = await _context.AIAgentAssignments
            .Include(aa => aa.Agent)
            .Include(aa => aa.AssignedByUser)
            .Where(aa => aa.EntityType == entityType
                && aa.EntityId == entityId
                && aa.CompanyId == companyId
                && aa.IsActive)
            .Select(aa => new AIAgentAssignmentDto
            {
                Id = aa.Id,
                AgentId = aa.AgentId,
                AgentName = aa.Agent.Name,
                EntityType = aa.EntityType,
                EntityId = aa.EntityId,
                AssignedByUserId = aa.AssignedByUserId,
                AssignedByUserName = aa.AssignedByUser.Name,
                AssignedAt = aa.AssignedAt,
                IsActive = aa.IsActive,
                Notes = aa.Notes,
                CreatedAt = aa.CreatedAt
            })
            .ToListAsync();

        return assignments;
    }

    private async Task<AIAgentAssignmentDto?> GetAssignmentByIdAsync(int id)
    {
        var assignment = await _context.AIAgentAssignments
            .Include(aa => aa.Agent)
            .Include(aa => aa.AssignedByUser)
            .FirstOrDefaultAsync(aa => aa.Id == id);

        if (assignment == null) return null;

        return new AIAgentAssignmentDto
        {
            Id = assignment.Id,
            AgentId = assignment.AgentId,
            AgentName = assignment.Agent.Name,
            EntityType = assignment.EntityType,
            EntityId = assignment.EntityId,
            AssignedByUserId = assignment.AssignedByUserId,
            AssignedByUserName = assignment.AssignedByUser.Name,
            AssignedAt = assignment.AssignedAt,
            IsActive = assignment.IsActive,
            Notes = assignment.Notes,
            CreatedAt = assignment.CreatedAt
        };
    }
}

