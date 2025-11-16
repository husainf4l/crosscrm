using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.Services;

public interface ISearchService
{
    Task<SearchResponseDto> SearchAsync(SearchQueryDto query, int companyId, int? userId = null);
    Task<List<SearchSuggestionDto>> GetSearchSuggestionsAsync(string query, int companyId, int? userId = null, int limit = 10);
}

