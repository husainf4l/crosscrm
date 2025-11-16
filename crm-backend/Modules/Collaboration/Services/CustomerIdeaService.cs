using crm_backend.Data;
using crm_backend.Modules.Collaboration.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Collaboration.Services;

public class CustomerIdeaService : ICustomerIdeaService
{
    private readonly CrmDbContext _context;

    public CustomerIdeaService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomerIdeaDto>> GetIdeasByCustomerAsync(int customerId, int companyId, int? userId = null)
    {
        var ideas = await _context.CustomerIdeas
            .Include(ci => ci.Customer)
            .Include(ci => ci.CreatedByUser)
            .Where(ci => ci.CustomerId == customerId && ci.CompanyId == companyId)
            .Select(ci => new CustomerIdeaDto
            {
                Id = ci.Id,
                Title = ci.Title,
                Description = ci.Description,
                Category = ci.Category.ToString(),
                Status = ci.Status.ToString(),
                Priority = ci.Priority.ToString(),
                Upvotes = ci.Upvotes,
                Downvotes = ci.Downvotes,
                CustomerId = ci.CustomerId,
                CustomerName = ci.Customer.Name,
                WorkspaceId = ci.WorkspaceId,
                CreatedByUserId = ci.CreatedByUserId,
                CreatedByUserName = ci.CreatedByUser.Name,
                CompanyId = ci.CompanyId,
                CreatedAt = ci.CreatedAt,
                UpdatedAt = ci.UpdatedAt,
                CommentCount = _context.NoteComments.Count(nc => nc.EntityType == "Idea" && nc.EntityId == ci.Id),
                UserVote = userId.HasValue
                    ? _context.IdeaVotes
                        .Where(iv => iv.IdeaId == ci.Id && iv.UserId == userId.Value)
                        .Select(iv => (bool?)iv.IsUpvote)
                        .FirstOrDefault()
                    : null
            })
            .ToListAsync();

        return ideas;
    }

    public async Task<IEnumerable<CustomerIdeaDto>> GetIdeasByWorkspaceAsync(int workspaceId, int companyId, int? userId = null)
    {
        var ideas = await _context.CustomerIdeas
            .Include(ci => ci.Customer)
            .Include(ci => ci.CreatedByUser)
            .Where(ci => ci.WorkspaceId == workspaceId && ci.CompanyId == companyId)
            .Select(ci => new CustomerIdeaDto
            {
                Id = ci.Id,
                Title = ci.Title,
                Description = ci.Description,
                Category = ci.Category.ToString(),
                Status = ci.Status.ToString(),
                Priority = ci.Priority.ToString(),
                Upvotes = ci.Upvotes,
                Downvotes = ci.Downvotes,
                CustomerId = ci.CustomerId,
                CustomerName = ci.Customer.Name,
                WorkspaceId = ci.WorkspaceId,
                CreatedByUserId = ci.CreatedByUserId,
                CreatedByUserName = ci.CreatedByUser.Name,
                CompanyId = ci.CompanyId,
                CreatedAt = ci.CreatedAt,
                UpdatedAt = ci.UpdatedAt,
                CommentCount = _context.NoteComments.Count(nc => nc.EntityType == "Idea" && nc.EntityId == ci.Id),
                UserVote = userId.HasValue
                    ? _context.IdeaVotes
                        .Where(iv => iv.IdeaId == ci.Id && iv.UserId == userId.Value)
                        .Select(iv => (bool?)iv.IsUpvote)
                        .FirstOrDefault()
                    : null
            })
            .ToListAsync();

        return ideas;
    }

    public async Task<CustomerIdeaDto?> GetIdeaByIdAsync(int id, int? userId = null)
    {
        var idea = await _context.CustomerIdeas
            .Include(ci => ci.Customer)
            .Include(ci => ci.CreatedByUser)
            .FirstOrDefaultAsync(ci => ci.Id == id);

        if (idea == null) return null;

        var commentCount = await _context.NoteComments
            .CountAsync(nc => nc.EntityType == "Idea" && nc.EntityId == idea.Id);

        var userVote = userId.HasValue
            ? await _context.IdeaVotes
                .Where(iv => iv.IdeaId == idea.Id && iv.UserId == userId.Value)
                .Select(iv => (bool?)iv.IsUpvote)
                .FirstOrDefaultAsync()
            : null;

        return new CustomerIdeaDto
        {
            Id = idea.Id,
            Title = idea.Title,
            Description = idea.Description,
            Category = idea.Category.ToString(),
            Status = idea.Status.ToString(),
            Priority = idea.Priority.ToString(),
            Upvotes = idea.Upvotes,
            Downvotes = idea.Downvotes,
            CustomerId = idea.CustomerId,
            CustomerName = idea.Customer.Name,
            WorkspaceId = idea.WorkspaceId,
            CreatedByUserId = idea.CreatedByUserId,
            CreatedByUserName = idea.CreatedByUser.Name,
            CompanyId = idea.CompanyId,
            CreatedAt = idea.CreatedAt,
            UpdatedAt = idea.UpdatedAt,
            CommentCount = commentCount,
            UserVote = userVote
        };
    }

    public async Task<CustomerIdeaDto> CreateIdeaAsync(CreateCustomerIdeaDto dto, int companyId, int createdByUserId)
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

        // Parse enums
        if (!Enum.TryParse<IdeaCategory>(dto.Category, true, out var category))
        {
            throw new ArgumentException($"Invalid category: {dto.Category}");
        }

        if (!Enum.TryParse<IdeaPriority>(dto.Priority, true, out var priority))
        {
            throw new ArgumentException($"Invalid priority: {dto.Priority}");
        }

        var idea = new CustomerIdea
        {
            CustomerId = dto.CustomerId,
            WorkspaceId = dto.WorkspaceId,
            Title = dto.Title,
            Description = dto.Description,
            Category = category,
            Status = IdeaStatus.New,
            Priority = priority,
            Upvotes = 0,
            Downvotes = 0,
            CreatedByUserId = createdByUserId,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.CustomerIdeas.Add(idea);
        await _context.SaveChangesAsync();

        return await GetIdeaByIdAsync(idea.Id)
            ?? throw new InvalidOperationException("Failed to retrieve created idea");
    }

    public async Task<CustomerIdeaDto?> UpdateIdeaAsync(int id, UpdateCustomerIdeaDto dto, int companyId)
    {
        var idea = await _context.CustomerIdeas
            .FirstOrDefaultAsync(ci => ci.Id == id && ci.CompanyId == companyId);
        if (idea == null) return null;

        if (!string.IsNullOrEmpty(dto.Title))
            idea.Title = dto.Title;

        if (dto.Description != null)
            idea.Description = dto.Description;

        if (!string.IsNullOrEmpty(dto.Category) && Enum.TryParse<IdeaCategory>(dto.Category, true, out var category))
            idea.Category = category;

        if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<IdeaStatus>(dto.Status, true, out var status))
            idea.Status = status;

        if (!string.IsNullOrEmpty(dto.Priority) && Enum.TryParse<IdeaPriority>(dto.Priority, true, out var priority))
            idea.Priority = priority;

        idea.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetIdeaByIdAsync(idea.Id);
    }

    public async Task<bool> DeleteIdeaAsync(int id, int companyId)
    {
        var idea = await _context.CustomerIdeas
            .FirstOrDefaultAsync(ci => ci.Id == id && ci.CompanyId == companyId);
        if (idea == null) return false;

        _context.CustomerIdeas.Remove(idea);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<CustomerIdeaDto> VoteIdeaAsync(VoteIdeaDto dto, int userId, int companyId)
    {
        var idea = await _context.CustomerIdeas
            .FirstOrDefaultAsync(ci => ci.Id == dto.IdeaId && ci.CompanyId == companyId);
        if (idea == null)
        {
            throw new InvalidOperationException("Idea not found or does not belong to the company.");
        }

        // Check if user already voted
        var existingVote = await _context.IdeaVotes
            .FirstOrDefaultAsync(iv => iv.IdeaId == dto.IdeaId && iv.UserId == userId);

        if (existingVote != null)
        {
            // Update existing vote
            if (existingVote.IsUpvote != dto.IsUpvote)
            {
                // Vote changed
                if (existingVote.IsUpvote)
                {
                    idea.Upvotes--;
                    idea.Downvotes++;
                }
                else
                {
                    idea.Downvotes--;
                    idea.Upvotes++;
                }
                existingVote.IsUpvote = dto.IsUpvote;
            }
            // If same vote, do nothing (or remove vote)
        }
        else
        {
            // Create new vote
            var vote = new IdeaVote
            {
                IdeaId = dto.IdeaId,
                UserId = userId,
                IsUpvote = dto.IsUpvote,
                CreatedAt = DateTime.UtcNow
            };
            _context.IdeaVotes.Add(vote);

            if (dto.IsUpvote)
                idea.Upvotes++;
            else
                idea.Downvotes++;
        }

        await _context.SaveChangesAsync();

        return await GetIdeaByIdAsync(idea.Id, userId)
            ?? throw new InvalidOperationException("Failed to retrieve idea");
    }

    public async Task<bool> RemoveVoteAsync(int ideaId, int userId, int companyId)
    {
        var idea = await _context.CustomerIdeas
            .FirstOrDefaultAsync(ci => ci.Id == ideaId && ci.CompanyId == companyId);
        if (idea == null)
        {
            throw new InvalidOperationException("Idea not found or does not belong to the company.");
        }

        var vote = await _context.IdeaVotes
            .FirstOrDefaultAsync(iv => iv.IdeaId == ideaId && iv.UserId == userId);
        if (vote == null) return false;

        if (vote.IsUpvote)
            idea.Upvotes--;
        else
            idea.Downvotes--;

        _context.IdeaVotes.Remove(vote);
        await _context.SaveChangesAsync();

        return true;
    }
}

