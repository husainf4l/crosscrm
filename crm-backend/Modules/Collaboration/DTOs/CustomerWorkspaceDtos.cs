using System.Text.Json;
using HotChocolate;

namespace crm_backend.Modules.Collaboration.DTOs;

public class CustomerWorkspaceDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? Summary { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
    public int? LastUpdatedByUserId { get; set; }
    public string? LastUpdatedByUserName { get; set; }
    public int CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int StrategyCount { get; set; }
    public int IdeaCount { get; set; }
}

public class CreateCustomerWorkspaceDto
{
    public int CustomerId { get; set; }
    public int? TeamId { get; set; }
    public string? Summary { get; set; }
}

public class UpdateCustomerWorkspaceDto
{
    public int? TeamId { get; set; }
    public string? Summary { get; set; }
}

public class CustomerStrategyDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    [GraphQLIgnore]
    public Dictionary<string, object>? SuccessMetrics { get; set; }
    
    public string? SuccessMetricsJson => SuccessMetrics != null 
        ? JsonSerializer.Serialize(SuccessMetrics) 
        : null;
    
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? WorkspaceId { get; set; }
    public int CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public int? AssignedToTeamId { get; set; }
    public string? AssignedTeamName { get; set; }
    public int CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int CommentCount { get; set; }
}

public class CreateCustomerStrategyDto
{
    public int CustomerId { get; set; }
    public int? WorkspaceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty; // StrategyType enum as string
    public string Priority { get; set; } = string.Empty; // StrategyPriority enum as string
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? AssignedToTeamId { get; set; }
    
    [GraphQLIgnore]
    public Dictionary<string, object>? SuccessMetrics { get; set; }
    
    public string? SuccessMetricsJson
    {
        get => SuccessMetrics != null ? JsonSerializer.Serialize(SuccessMetrics) : null;
        set => SuccessMetrics = !string.IsNullOrEmpty(value) 
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(value) 
            : null;
    }
}

public class UpdateCustomerStrategyDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? AssignedToTeamId { get; set; }
    
    [GraphQLIgnore]
    public Dictionary<string, object>? SuccessMetrics { get; set; }
    
    public string? SuccessMetricsJson
    {
        get => SuccessMetrics != null ? JsonSerializer.Serialize(SuccessMetrics) : null;
        set => SuccessMetrics = !string.IsNullOrEmpty(value) 
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(value) 
            : null;
    }
}

public class CustomerIdeaDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int Upvotes { get; set; }
    public int Downvotes { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? WorkspaceId { get; set; }
    public int CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public int CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int CommentCount { get; set; }
    public bool? UserVote { get; set; } // null = no vote, true = upvote, false = downvote
}

public class CreateCustomerIdeaDto
{
    public int CustomerId { get; set; }
    public int? WorkspaceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty; // IdeaCategory enum as string
    public string Priority { get; set; } = string.Empty; // IdeaPriority enum as string
}

public class UpdateCustomerIdeaDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
}

public class NoteCommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int? ParentCommentId { get; set; }
    public int CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ReplyCount { get; set; }
}

public class CreateNoteCommentDto
{
    public string EntityType { get; set; } = string.Empty; // "Note", "Strategy", "Idea"
    public int EntityId { get; set; }
    public int? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class UpdateNoteCommentDto
{
    public string Content { get; set; } = string.Empty;
}

public class VoteIdeaDto
{
    public int IdeaId { get; set; }
    public bool IsUpvote { get; set; } // true for upvote, false for downvote
}

