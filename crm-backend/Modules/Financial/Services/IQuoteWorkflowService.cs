using crm_backend.Modules.Financial.DTOs;

namespace crm_backend.Modules.Financial.Services;

public interface IQuoteWorkflowService
{
    Task<QuoteDto> TransitionQuoteStatusAsync(int quoteId, QuoteStatus newStatus, int userId, int companyId);
    Task<InvoiceDto> ConvertQuoteToInvoiceAsync(int quoteId, int userId, int companyId);
    Task<bool> ValidateQuoteStatusTransitionAsync(QuoteStatus fromStatus, QuoteStatus toStatus);
}

