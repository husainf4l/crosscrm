using crm_backend.Data;
using crm_backend.GraphQL;
using crm_backend.Modules.Collaboration.DTOs;
using crm_backend.Modules.Collaboration.Services;
using Microsoft.AspNetCore.Authorization;

namespace crm_backend.Modules.Collaboration;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class SearchResolver : BaseResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<SearchResponseDto> Search(
        string query,
        [Service] ISearchService searchService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        List<string>? entityTypes = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? createdByUserId = null,
        int? teamId = null,
        int? limit = null,
        int? offset = null)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var userId = GetUserId(httpContextAccessor.HttpContext);

        var searchQuery = new SearchQueryDto
        {
            Query = query ?? string.Empty,
            EntityTypes = entityTypes,
            StartDate = startDate,
            EndDate = endDate,
            CreatedByUserId = createdByUserId,
            TeamId = teamId,
            Limit = limit,
            Offset = offset
        };

        return await searchService.SearchAsync(searchQuery, companyId, userId);
    }

    [Authorize]
    public async Task<List<SearchSuggestionDto>> GetSearchSuggestions(
        string query,
        [Service] ISearchService searchService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        int? limit = null)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var userId = GetUserId(httpContextAccessor.HttpContext);

        return await searchService.GetSearchSuggestionsAsync(
            query ?? string.Empty,
            companyId,
            userId,
            limit ?? 10);
    }
}

