namespace crm_backend.Modules.Collaboration.DTOs;

public class SearchResultDto
{
    public string EntityType { get; set; } = string.Empty; // "Customer", "Message", "Note", "Strategy", "Idea", "Activity"
    public int EntityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? HighlightedText { get; set; } // Text with search terms highlighted
    public string? ActionUrl { get; set; } // URL to navigate to the entity

    [GraphQLIgnore]
    public Dictionary<string, object>? Metadata { get; set; } // Additional entity-specific data

    public DateTime? CreatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public double RelevanceScore { get; set; } // Search relevance score (0-1)
}

public class SearchQueryDto
{
    public string Query { get; set; } = string.Empty;
    public List<string>? EntityTypes { get; set; } // Filter by entity types
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? CreatedByUserId { get; set; } // Filter by creator
    public int? TeamId { get; set; } // Filter by team
    public int? Limit { get; set; } // Max results per entity type
    public int? Offset { get; set; } // Pagination offset
}

public class SearchResponseDto
{
    public List<SearchResultDto> Results { get; set; } = new();
    public int TotalCount { get; set; }

    [GraphQLIgnore]
    public Dictionary<string, int> CountByEntityType { get; set; } = new(); // Count per entity type

    public List<string> Suggestions { get; set; } = new(); // Search suggestions
}

public class SearchSuggestionDto
{
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Customer", "User", "Channel", etc.
    public int? EntityId { get; set; }
    public string? ActionUrl { get; set; }
}

