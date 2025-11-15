using System.Text.RegularExpressions;
using crm_backend.Data;
using crm_backend.Modules.Collaboration.DTOs;
using crm_backend.Modules.Customer;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Collaboration.Services;

public class SearchService : ISearchService
{
    private readonly CrmDbContext _context;

    public SearchService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<SearchResponseDto> SearchAsync(SearchQueryDto query, int companyId, int? userId = null)
    {
        var results = new List<SearchResultDto>();
        var countByEntityType = new Dictionary<string, int>();

        if (string.IsNullOrWhiteSpace(query.Query))
        {
            return new SearchResponseDto
            {
                Results = results,
                TotalCount = 0,
                CountByEntityType = countByEntityType
            };
        }

        var searchTerms = query.Query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var entityTypes = query.EntityTypes ?? new List<string> { "Customer", "Message", "Note", "Strategy", "Idea", "Activity" };

        // If teamId is specified, get team member user IDs
        List<int>? teamUserIds = null;
        if (query.TeamId.HasValue)
        {
            teamUserIds = await _context.TeamMembers
                .Where(tm => tm.TeamId == query.TeamId.Value && tm.Team.CompanyId == companyId)
                .Select(tm => tm.UserId)
                .ToListAsync();
            
            if (!teamUserIds.Any())
            {
                // No team members, return empty results
                return new SearchResponseDto
                {
                    Results = new List<SearchResultDto>(),
                    TotalCount = 0,
                    CountByEntityType = new Dictionary<string, int>()
                };
            }
        }

        // Search Customers
        if (entityTypes.Contains("Customer"))
        {
            var customerResults = await SearchCustomersAsync(searchTerms, companyId, query, userId, teamUserIds);
            results.AddRange(customerResults);
            countByEntityType["Customer"] = customerResults.Count;
        }

        // Search Messages
        if (entityTypes.Contains("Message"))
        {
            var messageResults = await SearchMessagesAsync(searchTerms, companyId, query, userId, teamUserIds);
            results.AddRange(messageResults);
            countByEntityType["Message"] = messageResults.Count;
        }

        // Search Notes
        if (entityTypes.Contains("Note"))
        {
            var noteResults = await SearchNotesAsync(searchTerms, companyId, query, userId, teamUserIds);
            results.AddRange(noteResults);
            countByEntityType["Note"] = noteResults.Count;
        }

        // Search Strategies
        if (entityTypes.Contains("Strategy"))
        {
            var strategyResults = await SearchStrategiesAsync(searchTerms, companyId, query, userId, teamUserIds);
            results.AddRange(strategyResults);
            countByEntityType["Strategy"] = strategyResults.Count;
        }

        // Search Ideas
        if (entityTypes.Contains("Idea"))
        {
            var ideaResults = await SearchIdeasAsync(searchTerms, companyId, query, userId, teamUserIds);
            results.AddRange(ideaResults);
            countByEntityType["Idea"] = ideaResults.Count;
        }

        // Search Activities
        if (entityTypes.Contains("Activity"))
        {
            var activityResults = await SearchActivitiesAsync(searchTerms, companyId, query, userId, teamUserIds);
            results.AddRange(activityResults);
            countByEntityType["Activity"] = activityResults.Count;
        }

        // Sort by relevance score (descending)
        results = results.OrderByDescending(r => r.RelevanceScore).ToList();

        // Apply pagination
        if (query.Offset.HasValue || query.Limit.HasValue)
        {
            var offset = query.Offset ?? 0;
            var limit = query.Limit ?? 50;
            results = results.Skip(offset).Take(limit).ToList();
        }

        // Get suggestions
        var suggestions = await GetSearchSuggestionsAsync(query.Query, companyId, userId, 5);

        return new SearchResponseDto
        {
            Results = results,
            TotalCount = results.Count,
            CountByEntityType = countByEntityType,
            Suggestions = suggestions.Select(s => s.Text).ToList()
        };
    }

    public async Task<List<SearchSuggestionDto>> GetSearchSuggestionsAsync(string query, int companyId, int? userId = null, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return new List<SearchSuggestionDto>();
        }

        var suggestions = new List<SearchSuggestionDto>();
        var searchTerm = query.ToLower();

        // Customer name suggestions
        var customers = await _context.Customers
            .Where(c => c.CompanyId == companyId && 
                       (c.Name.ToLower().Contains(searchTerm) || 
                        (c.Email != null && c.Email.ToLower().Contains(searchTerm)) ||
                        (c.Phone != null && c.Phone.Contains(searchTerm))))
            .Take(5)
            .Select(c => new SearchSuggestionDto
            {
                Text = c.Name,
                Type = "Customer",
                EntityId = c.Id,
                ActionUrl = $"/customers/{c.Id}"
            })
            .ToListAsync();

        suggestions.AddRange(customers);

        // User name suggestions
        var users = await _context.Users
            .Where(u => u.Name.ToLower().Contains(searchTerm) || 
                       u.Email.ToLower().Contains(searchTerm))
            .Take(3)
            .Select(u => new SearchSuggestionDto
            {
                Text = u.Name,
                Type = "User",
                EntityId = u.Id,
                ActionUrl = $"/users/{u.Id}"
            })
            .ToListAsync();

        suggestions.AddRange(users);

        // Channel name suggestions
        var channels = await _context.Channels
            .Where(ch => ch.CompanyId == companyId && 
                        ch.Name.ToLower().Contains(searchTerm))
            .Take(3)
            .Select(ch => new SearchSuggestionDto
            {
                Text = ch.Name,
                Type = "Channel",
                EntityId = ch.Id,
                ActionUrl = $"/channels/{ch.Id}"
            })
            .ToListAsync();

        suggestions.AddRange(channels);

        return suggestions.Take(limit).ToList();
    }

    private async Task<List<SearchResultDto>> SearchCustomersAsync(string[] searchTerms, int companyId, SearchQueryDto query, int? userId, List<int>? teamUserIds = null)
    {
        var customers = _context.Customers
            .Where(c => c.CompanyId == companyId);

        // Apply date filter
        if (query.StartDate.HasValue)
        {
            customers = customers.Where(c => c.CreatedAt >= query.StartDate.Value);
        }
        if (query.EndDate.HasValue)
        {
            customers = customers.Where(c => c.CreatedAt <= query.EndDate.Value);
        }

        // Apply search terms
        var filteredCustomers = await customers.ToListAsync();
        
        var results = new List<SearchResultDto>();
        foreach (var customer in filteredCustomers)
        {
            var searchableText = $"{customer.Name} {customer.Email} {customer.Phone} {customer.Address}".ToLower();
            var matches = searchTerms.Count(term => searchableText.Contains(term));
            
            if (matches > 0)
            {
                var relevanceScore = (double)matches / searchTerms.Length;
                var highlightedText = HighlightSearchTerms(customer.Name, searchTerms);

                results.Add(new SearchResultDto
                {
                    EntityType = "Customer",
                    EntityId = customer.Id,
                    Title = customer.Name,
                    Description = customer.Email,
                    HighlightedText = highlightedText,
                    ActionUrl = $"/customers/{customer.Id}",
                    Metadata = new Dictionary<string, object>
                    {
                        { "Email", customer.Email ?? "" },
                        { "Phone", customer.Phone ?? "" }
                    },
                    CreatedAt = customer.CreatedAt,
                    RelevanceScore = relevanceScore
                });
            }
        }

        return results.OrderByDescending(r => r.RelevanceScore).Take(query.Limit ?? 20).ToList();
    }

    private async Task<List<SearchResultDto>> SearchMessagesAsync(string[] searchTerms, int companyId, SearchQueryDto query, int? userId, List<int>? teamUserIds = null)
    {
        var messages = _context.Messages
            .Include(m => m.Channel)
            .Include(m => m.CreatedByUser)
            .Where(m => m.Channel.CompanyId == companyId);

        // Filter by user if specified
        if (query.CreatedByUserId.HasValue)
        {
            messages = messages.Where(m => m.CreatedByUserId == query.CreatedByUserId.Value);
        }

        // Filter by team if specified
        if (teamUserIds != null && teamUserIds.Any())
        {
            messages = messages.Where(m => m.CreatedByUserId.HasValue && teamUserIds.Contains(m.CreatedByUserId.Value));
        }

        // Apply date filter
        if (query.StartDate.HasValue)
        {
            messages = messages.Where(m => m.CreatedAt >= query.StartDate.Value);
        }
        if (query.EndDate.HasValue)
        {
            messages = messages.Where(m => m.CreatedAt <= query.EndDate.Value);
        }

        var filteredMessages = await messages.ToListAsync();

        var results = new List<SearchResultDto>();
        foreach (var message in filteredMessages)
        {
            var searchableText = $"{message.Content} {message.Channel.Name}".ToLower();
            var matches = searchTerms.Count(term => searchableText.Contains(term));

            if (matches > 0)
            {
                var relevanceScore = (double)matches / searchTerms.Length;
                var highlightedText = HighlightSearchTerms(message.Content, searchTerms);

                results.Add(new SearchResultDto
                {
                    EntityType = "Message",
                    EntityId = message.Id,
                    Title = $"Message in {message.Channel.Name}",
                    Description = message.Content.Length > 200 ? message.Content.Substring(0, 200) + "..." : message.Content,
                    HighlightedText = highlightedText,
                    ActionUrl = $"/channels/{message.ChannelId}/messages/{message.Id}",
                    Metadata = new Dictionary<string, object>
                    {
                        { "ChannelId", message.ChannelId },
                        { "ChannelName", message.Channel.Name }
                    },
                    CreatedAt = message.CreatedAt,
                    CreatedByUserId = message.CreatedByUserId,
                    CreatedByUserName = message.CreatedByUser?.Name,
                    RelevanceScore = relevanceScore
                });
            }
        }

        return results.OrderByDescending(r => r.RelevanceScore).Take(query.Limit ?? 20).ToList();
    }

    private async Task<List<SearchResultDto>> SearchNotesAsync(string[] searchTerms, int companyId, SearchQueryDto query, int? userId, List<int>? teamUserIds = null)
    {
        var notes = _context.CustomerNotes
            .Include(n => n.Customer)
            .Include(n => n.CreatedByUser)
            .Where(n => n.Customer.CompanyId == companyId);

        // Filter by user if specified
        if (query.CreatedByUserId.HasValue)
        {
            notes = notes.Where(n => n.CreatedByUserId == query.CreatedByUserId.Value);
        }

        // Filter by team if specified
        if (teamUserIds != null && teamUserIds.Any())
        {
            notes = notes.Where(n => teamUserIds.Contains(n.CreatedByUserId));
        }

        // Apply date filter
        if (query.StartDate.HasValue)
        {
            notes = notes.Where(n => n.CreatedAt >= query.StartDate.Value);
        }
        if (query.EndDate.HasValue)
        {
            notes = notes.Where(n => n.CreatedAt <= query.EndDate.Value);
        }

        var filteredNotes = await notes.ToListAsync();

        var results = new List<SearchResultDto>();
        foreach (var note in filteredNotes)
        {
            var searchableText = $"{(note.Title ?? "")} {note.Content} {note.Customer.Name}".ToLower();
            var matches = searchTerms.Count(term => searchableText.Contains(term));

            if (matches > 0)
            {
                var relevanceScore = (double)matches / searchTerms.Length;
                var highlightedText = HighlightSearchTerms(note.Content, searchTerms);

                results.Add(new SearchResultDto
                {
                    EntityType = "Note",
                    EntityId = note.Id,
                    Title = note.Title ?? "Note",
                    Description = note.Content.Length > 200 ? note.Content.Substring(0, 200) + "..." : note.Content,
                    HighlightedText = highlightedText,
                    ActionUrl = $"/customers/{note.CustomerId}/notes/{note.Id}",
                    Metadata = new Dictionary<string, object>
                    {
                        { "CustomerId", note.CustomerId },
                        { "CustomerName", note.Customer.Name }
                    },
                    CreatedAt = note.CreatedAt,
                    CreatedByUserId = note.CreatedByUserId,
                    CreatedByUserName = note.CreatedByUser?.Name,
                    RelevanceScore = relevanceScore
                });
            }
        }

        return results.OrderByDescending(r => r.RelevanceScore).Take(query.Limit ?? 20).ToList();
    }

    private async Task<List<SearchResultDto>> SearchStrategiesAsync(string[] searchTerms, int companyId, SearchQueryDto query, int? userId, List<int>? teamUserIds = null)
    {
        var strategies = _context.CustomerStrategies
            .Include(s => s.Customer)
            .Include(s => s.CreatedByUser)
            .Where(s => s.CompanyId == companyId);

        // Filter by user if specified
        if (query.CreatedByUserId.HasValue)
        {
            strategies = strategies.Where(s => s.CreatedByUserId == query.CreatedByUserId.Value);
        }

        // Filter by team if specified
        if (teamUserIds != null && teamUserIds.Any())
        {
            strategies = strategies.Where(s => teamUserIds.Contains(s.CreatedByUserId));
        }

        // Apply date filter
        if (query.StartDate.HasValue)
        {
            strategies = strategies.Where(s => s.CreatedAt >= query.StartDate.Value);
        }
        if (query.EndDate.HasValue)
        {
            strategies = strategies.Where(s => s.CreatedAt <= query.EndDate.Value);
        }

        var filteredStrategies = await strategies.ToListAsync();

        var results = new List<SearchResultDto>();
        foreach (var strategy in filteredStrategies)
        {
            var searchableText = $"{strategy.Title} {strategy.Description} {strategy.Customer.Name}".ToLower();
            var matches = searchTerms.Count(term => searchableText.Contains(term));

            if (matches > 0)
            {
                var relevanceScore = (double)matches / searchTerms.Length;
                var highlightedText = HighlightSearchTerms(strategy.Description ?? "", searchTerms);

                results.Add(new SearchResultDto
                {
                    EntityType = "Strategy",
                    EntityId = strategy.Id,
                    Title = strategy.Title,
                    Description = strategy.Description,
                    HighlightedText = highlightedText,
                    ActionUrl = $"/customers/{strategy.CustomerId}/strategies/{strategy.Id}",
                    Metadata = new Dictionary<string, object>
                    {
                        { "CustomerId", strategy.CustomerId },
                        { "CustomerName", strategy.Customer.Name },
                        { "Status", strategy.Status.ToString() }
                    },
                    CreatedAt = strategy.CreatedAt,
                    CreatedByUserId = strategy.CreatedByUserId,
                    CreatedByUserName = strategy.CreatedByUser?.Name,
                    RelevanceScore = relevanceScore
                });
            }
        }

        return results.OrderByDescending(r => r.RelevanceScore).Take(query.Limit ?? 20).ToList();
    }

    private async Task<List<SearchResultDto>> SearchIdeasAsync(string[] searchTerms, int companyId, SearchQueryDto query, int? userId, List<int>? teamUserIds = null)
    {
        var ideas = _context.CustomerIdeas
            .Include(i => i.Customer)
            .Include(i => i.CreatedByUser)
            .Where(i => i.CompanyId == companyId);

        // Filter by user if specified
        if (query.CreatedByUserId.HasValue)
        {
            ideas = ideas.Where(i => i.CreatedByUserId == query.CreatedByUserId.Value);
        }

        // Filter by team if specified
        if (teamUserIds != null && teamUserIds.Any())
        {
            ideas = ideas.Where(i => teamUserIds.Contains(i.CreatedByUserId));
        }

        // Apply date filter
        if (query.StartDate.HasValue)
        {
            ideas = ideas.Where(i => i.CreatedAt >= query.StartDate.Value);
        }
        if (query.EndDate.HasValue)
        {
            ideas = ideas.Where(i => i.CreatedAt <= query.EndDate.Value);
        }

        var filteredIdeas = await ideas.ToListAsync();

        var results = new List<SearchResultDto>();
        foreach (var idea in filteredIdeas)
        {
            var searchableText = $"{idea.Title} {idea.Description} {idea.Customer.Name}".ToLower();
            var matches = searchTerms.Count(term => searchableText.Contains(term));

            if (matches > 0)
            {
                var relevanceScore = (double)matches / searchTerms.Length;
                var highlightedText = HighlightSearchTerms(idea.Description ?? "", searchTerms);

                results.Add(new SearchResultDto
                {
                    EntityType = "Idea",
                    EntityId = idea.Id,
                    Title = idea.Title,
                    Description = idea.Description,
                    HighlightedText = highlightedText,
                    ActionUrl = $"/customers/{idea.CustomerId}/ideas/{idea.Id}",
                    Metadata = new Dictionary<string, object>
                    {
                        { "CustomerId", idea.CustomerId },
                        { "CustomerName", idea.Customer.Name },
                        { "Upvotes", idea.Upvotes },
                        { "Downvotes", idea.Downvotes }
                    },
                    CreatedAt = idea.CreatedAt,
                    CreatedByUserId = idea.CreatedByUserId,
                    CreatedByUserName = idea.CreatedByUser?.Name,
                    RelevanceScore = relevanceScore
                });
            }
        }

        return results.OrderByDescending(r => r.RelevanceScore).Take(query.Limit ?? 20).ToList();
    }

    private async Task<List<SearchResultDto>> SearchActivitiesAsync(string[] searchTerms, int companyId, SearchQueryDto query, int? userId, List<int>? teamUserIds = null)
    {
        var activities = _context.ActivityTimelines
            .Include(a => a.PerformedByUser)
            .Where(a => a.CompanyId == companyId);

        // Filter by user if specified
        if (query.CreatedByUserId.HasValue)
        {
            activities = activities.Where(a => a.PerformedByUserId == query.CreatedByUserId.Value);
        }

        // Filter by team if specified
        if (teamUserIds != null && teamUserIds.Any())
        {
            activities = activities.Where(a => a.PerformedByUserId.HasValue && teamUserIds.Contains(a.PerformedByUserId.Value));
        }

        // Apply date filter
        if (query.StartDate.HasValue)
        {
            activities = activities.Where(a => a.CreatedAt >= query.StartDate.Value);
        }
        if (query.EndDate.HasValue)
        {
            activities = activities.Where(a => a.CreatedAt <= query.EndDate.Value);
        }

        var filteredActivities = await activities.ToListAsync();

        var results = new List<SearchResultDto>();
        foreach (var activity in filteredActivities)
        {
            var searchableText = $"{activity.Description} {activity.Type}".ToLower();
            var matches = searchTerms.Count(term => searchableText.Contains(term));

            if (matches > 0)
            {
                var relevanceScore = (double)matches / searchTerms.Length;
                var highlightedText = HighlightSearchTerms(activity.Description ?? "", searchTerms);

                results.Add(new SearchResultDto
                {
                    EntityType = "Activity",
                    EntityId = activity.Id,
                    Title = activity.Type.ToString(),
                    Description = activity.Description,
                    HighlightedText = highlightedText,
                    ActionUrl = $"/activities/{activity.Id}",
                    Metadata = new Dictionary<string, object>
                    {
                        { "EntityType", activity.EntityType },
                        { "EntityId", activity.EntityId }
                    },
                    CreatedAt = activity.CreatedAt,
                    CreatedByUserId = activity.PerformedByUserId,
                    CreatedByUserName = activity.PerformedByUser?.Name,
                    RelevanceScore = relevanceScore
                });
            }
        }

        return results.OrderByDescending(r => r.RelevanceScore).Take(query.Limit ?? 20).ToList();
    }

    private string HighlightSearchTerms(string text, string[] searchTerms)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var highlightedText = text;
        foreach (var term in searchTerms)
        {
            var pattern = Regex.Escape(term);
            highlightedText = Regex.Replace(
                highlightedText,
                pattern,
                match => $"<mark>{match.Value}</mark>",
                RegexOptions.IgnoreCase);
        }

        return highlightedText;
    }
}

