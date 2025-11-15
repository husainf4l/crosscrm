using crm_backend.Modules.Financial.DTOs;

namespace crm_backend.Modules.Financial.Services;

public interface IQuoteService
{
    Task<IEnumerable<QuoteDto>> GetAllQuotesAsync(int? companyId = null);
    Task<QuoteDto?> GetQuoteByIdAsync(int id);
    Task<QuoteDto> CreateQuoteAsync(CreateQuoteDto dto);
    Task<QuoteDto?> UpdateQuoteAsync(int id, UpdateQuoteDto dto);
    Task<bool> DeleteQuoteAsync(int id);
    Task<QuoteDto?> ConvertQuoteToInvoiceAsync(int quoteId);
    Task<IEnumerable<QuoteDto>> GetQuotesByOpportunityAsync(int opportunityId, int companyId);
}

