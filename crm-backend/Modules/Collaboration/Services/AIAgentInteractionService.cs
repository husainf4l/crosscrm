using crm_backend.Data;
using crm_backend.Modules.Collaboration;
using crm_backend.Modules.Collaboration.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Collaboration.Services;

public class AIAgentInteractionService : IAIAgentInteractionService
{
    private readonly CrmDbContext _context;

    public AIAgentInteractionService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<AIAgentInteractionDto> CreateInteractionAsync(CreateAIAgentInteractionDto dto, int companyId, int? userId = null)
    {
        // Verify agent exists and belongs to company
        var agent = await _context.AIAgents
            .FirstOrDefaultAsync(a => a.Id == dto.AgentId && a.CompanyId == companyId);
        if (agent == null)
        {
            throw new InvalidOperationException("Agent not found or does not belong to the company.");
        }

        // Parse interaction type
        if (!Enum.TryParse<InteractionType>(dto.Type, true, out var interactionType))
        {
            throw new ArgumentException($"Invalid interaction type: {dto.Type}");
        }

        var interaction = new AIAgentInteraction
        {
            AgentId = dto.AgentId,
            Type = interactionType,
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            Input = dto.Input,
            Status = InteractionStatus.Pending,
            UserId = userId,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.AIAgentInteractions.Add(interaction);
        await _context.SaveChangesAsync();

        return await GetInteractionByIdAsync(interaction.Id) ?? throw new InvalidOperationException("Failed to retrieve created interaction");
    }

    public async Task<AIAgentInteractionDto?> UpdateInteractionAsync(int id, UpdateAIAgentInteractionDto dto)
    {
        var interaction = await _context.AIAgentInteractions.FindAsync(id);
        if (interaction == null) return null;

        if (!string.IsNullOrEmpty(dto.Output))
            interaction.Output = dto.Output;

        if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<InteractionStatus>(dto.Status, true, out var status))
        {
            interaction.Status = status;
            if (status == InteractionStatus.Success || status == InteractionStatus.Failed)
            {
                interaction.CompletedAt = DateTime.UtcNow;
            }
        }

        if (!string.IsNullOrEmpty(dto.RequestId))
            interaction.RequestId = dto.RequestId;

        await _context.SaveChangesAsync();

        return await GetInteractionByIdAsync(interaction.Id);
    }

    public async Task<AIAgentInteractionDto?> GetInteractionByIdAsync(int id)
    {
        var interaction = await _context.AIAgentInteractions
            .Include(ai => ai.Agent)
            .Include(ai => ai.User)
            .FirstOrDefaultAsync(ai => ai.Id == id);

        if (interaction == null) return null;

        return new AIAgentInteractionDto
        {
            Id = interaction.Id,
            AgentId = interaction.AgentId,
            AgentName = interaction.Agent.Name,
            Type = interaction.Type.ToString(),
            EntityType = interaction.EntityType,
            EntityId = interaction.EntityId,
            Input = interaction.Input,
            Output = interaction.Output,
            Status = interaction.Status.ToString(),
            RequestId = interaction.RequestId,
            UserId = interaction.UserId,
            UserName = interaction.User?.Name,
            CreatedAt = interaction.CreatedAt,
            CompletedAt = interaction.CompletedAt
        };
    }

    public async Task<IEnumerable<AIAgentInteractionDto>> GetInteractionsByAgentAsync(int agentId, int? companyId = null)
    {
        var query = _context.AIAgentInteractions
            .Include(ai => ai.Agent)
            .Include(ai => ai.User)
            .Where(ai => ai.AgentId == agentId)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(ai => ai.CompanyId == companyId.Value);
        }

        var interactions = await query
            .OrderByDescending(ai => ai.CreatedAt)
            .Select(ai => new AIAgentInteractionDto
            {
                Id = ai.Id,
                AgentId = ai.AgentId,
                AgentName = ai.Agent.Name,
                Type = ai.Type.ToString(),
                EntityType = ai.EntityType,
                EntityId = ai.EntityId,
                Input = ai.Input,
                Output = ai.Output,
                Status = ai.Status.ToString(),
                RequestId = ai.RequestId,
                UserId = ai.UserId,
                UserName = ai.User != null ? ai.User.Name : null,
                CreatedAt = ai.CreatedAt,
                CompletedAt = ai.CompletedAt
            })
            .ToListAsync();

        return interactions;
    }

    public async Task<IEnumerable<AIAgentInteractionDto>> GetInteractionsByEntityAsync(string entityType, int entityId, int companyId)
    {
        var interactions = await _context.AIAgentInteractions
            .Include(ai => ai.Agent)
            .Include(ai => ai.User)
            .Where(ai => ai.EntityType == entityType 
                && ai.EntityId == entityId 
                && ai.CompanyId == companyId)
            .OrderByDescending(ai => ai.CreatedAt)
            .Select(ai => new AIAgentInteractionDto
            {
                Id = ai.Id,
                AgentId = ai.AgentId,
                AgentName = ai.Agent.Name,
                Type = ai.Type.ToString(),
                EntityType = ai.EntityType,
                EntityId = ai.EntityId,
                Input = ai.Input,
                Output = ai.Output,
                Status = ai.Status.ToString(),
                RequestId = ai.RequestId,
                UserId = ai.UserId,
                UserName = ai.User != null ? ai.User.Name : null,
                CreatedAt = ai.CreatedAt,
                CompletedAt = ai.CompletedAt
            })
            .ToListAsync();

        return interactions;
    }

    public async Task<IEnumerable<AIAgentInteractionDto>> GetInteractionsByUserAsync(int userId, int companyId)
    {
        var interactions = await _context.AIAgentInteractions
            .Include(ai => ai.Agent)
            .Include(ai => ai.User)
            .Where(ai => ai.UserId == userId && ai.CompanyId == companyId)
            .OrderByDescending(ai => ai.CreatedAt)
            .Select(ai => new AIAgentInteractionDto
            {
                Id = ai.Id,
                AgentId = ai.AgentId,
                AgentName = ai.Agent.Name,
                Type = ai.Type.ToString(),
                EntityType = ai.EntityType,
                EntityId = ai.EntityId,
                Input = ai.Input,
                Output = ai.Output,
                Status = ai.Status.ToString(),
                RequestId = ai.RequestId,
                UserId = ai.UserId,
                UserName = ai.User != null ? ai.User.Name : null,
                CreatedAt = ai.CreatedAt,
                CompletedAt = ai.CompletedAt
            })
            .ToListAsync();

        return interactions;
    }
}

