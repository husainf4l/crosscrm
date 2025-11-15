using crm_backend.Data;
using crm_backend.Modules.Collaboration.Services;
using crm_backend.Modules.Financial.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Financial.Services;

public class QuoteWorkflowService : IQuoteWorkflowService
{
    private readonly CrmDbContext _context;
    private readonly IQuoteService _quoteService;
    private readonly IInvoiceService _invoiceService;
    private readonly IActivityTimelineService _activityTimelineService;

    public QuoteWorkflowService(
        CrmDbContext context,
        IQuoteService quoteService,
        IInvoiceService invoiceService,
        IActivityTimelineService activityTimelineService)
    {
        _context = context;
        _quoteService = quoteService;
        _invoiceService = invoiceService;
        _activityTimelineService = activityTimelineService;
    }

    public async Task<bool> ValidateQuoteStatusTransitionAsync(QuoteStatus fromStatus, QuoteStatus toStatus)
    {
        // Define valid status transitions
        var validTransitions = new Dictionary<QuoteStatus, List<QuoteStatus>>
        {
            { QuoteStatus.Draft, new List<QuoteStatus> { QuoteStatus.Sent, QuoteStatus.Rejected } },
            { QuoteStatus.Sent, new List<QuoteStatus> { QuoteStatus.Accepted, QuoteStatus.Rejected, QuoteStatus.Expired } },
            { QuoteStatus.Accepted, new List<QuoteStatus> { QuoteStatus.Converted } },
            { QuoteStatus.Rejected, new List<QuoteStatus>() }, // Terminal state
            { QuoteStatus.Expired, new List<QuoteStatus>() }, // Terminal state
            { QuoteStatus.Converted, new List<QuoteStatus>() } // Terminal state
        };

        if (!validTransitions.ContainsKey(fromStatus))
            return false;

        return validTransitions[fromStatus].Contains(toStatus);
    }

    public async Task<QuoteDto> TransitionQuoteStatusAsync(int quoteId, QuoteStatus newStatus, int userId, int companyId)
    {
        var quote = await _context.Quotes
            .Include(q => q.Customer)
            .Include(q => q.Opportunity)
            .FirstOrDefaultAsync(q => q.Id == quoteId && q.CompanyId == companyId);

        if (quote == null)
            throw new InvalidOperationException("Quote not found");

        var oldStatus = quote.Status;

        // Validate transition
        if (!await ValidateQuoteStatusTransitionAsync(oldStatus, newStatus))
            throw new InvalidOperationException($"Invalid status transition from {oldStatus} to {newStatus}");

        // Update status
        quote.Status = newStatus;
        quote.UpdatedAt = DateTime.UtcNow;

        // Handle specific status changes
        if (newStatus == QuoteStatus.Sent && quote.SentAt == null)
        {
            quote.SentAt = DateTime.UtcNow;
        }
        else if (newStatus == QuoteStatus.Accepted && quote.AcceptedAt == null)
        {
            quote.AcceptedAt = DateTime.UtcNow;
        }
        else if (newStatus == QuoteStatus.Rejected && quote.RejectedAt == null)
        {
            quote.RejectedAt = DateTime.UtcNow;
        }
        else if (newStatus == QuoteStatus.Expired)
        {
            // Auto-expired by system
        }

        await _context.SaveChangesAsync();

        // Log activity
        await _activityTimelineService.LogActivityAsync(new Collaboration.DTOs.CreateActivityTimelineDto
        {
            EntityType = "Quote",
            EntityId = quoteId,
            Type = "StatusChange",
            Description = $"Quote status changed from {oldStatus} to {newStatus}"
        }, companyId, userId);

        return await _quoteService.GetQuoteByIdAsync(quoteId) 
            ?? throw new InvalidOperationException("Failed to retrieve updated quote");
    }

    public async Task<InvoiceDto> ConvertQuoteToInvoiceAsync(int quoteId, int userId, int companyId)
    {
        var quote = await _context.Quotes
            .Include(q => q.LineItems)
            .Include(q => q.Customer)
            .Include(q => q.Opportunity)
            .FirstOrDefaultAsync(q => q.Id == quoteId && q.CompanyId == companyId);

        if (quote == null)
            throw new InvalidOperationException("Quote not found");

        // Business rule: Quote must be Accepted before conversion
        if (quote.Status != QuoteStatus.Accepted)
            throw new InvalidOperationException($"Cannot convert quote with status {quote.Status}. Quote must be Accepted.");

        // Check if already converted
        if (quote.ConvertedToInvoiceId.HasValue)
            throw new InvalidOperationException("Quote has already been converted to an invoice.");

        // Mark quote as converted
        quote.Status = QuoteStatus.Converted;
        quote.UpdatedAt = DateTime.UtcNow;

        // Create invoice from quote
        var invoiceLineItems = quote.LineItems.Select(li => new InvoiceLineItemDto
        {
            ProductId = li.ProductId,
            Description = li.Description,
            Quantity = li.Quantity,
            UnitPrice = li.UnitPrice,
            DiscountPercent = li.DiscountPercent
        }).ToList();

        var createInvoiceDto = new CreateInvoiceDto
        {
            CustomerId = quote.CustomerId,
            QuoteId = quoteId,
            OpportunityId = quote.OpportunityId,
            Title = $"Invoice for {quote.Title ?? quote.QuoteNumber}",
            Description = quote.Description,
            TaxAmount = quote.TaxAmount,
            DiscountAmount = quote.DiscountAmount,
            Currency = quote.Currency,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30), // Default 30 days
            LineItems = quote.LineItems.Select(li => new CreateInvoiceLineItemDto
            {
                ProductId = li.ProductId,
                Description = li.Description,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice,
                DiscountPercent = li.DiscountPercent,
                Order = li.Order
            }).ToList(),
            CreatedByUserId = userId
        };

        var invoice = await _invoiceService.CreateInvoiceAsync(createInvoiceDto);

        // Link quote to invoice
        quote.ConvertedToInvoiceId = invoice.Id;
        await _context.SaveChangesAsync();

        // Log activity
        await _activityTimelineService.LogActivityAsync(new Collaboration.DTOs.CreateActivityTimelineDto
        {
            EntityType = "Quote",
            EntityId = quoteId,
            Type = "Converted",
            Description = $"Quote converted to Invoice {invoice.InvoiceNumber}"
        }, companyId, userId);

        await _activityTimelineService.LogActivityAsync(new Collaboration.DTOs.CreateActivityTimelineDto
        {
            EntityType = "Invoice",
            EntityId = invoice.Id,
            Type = "Created",
            Description = $"Invoice created from Quote {quote.QuoteNumber}"
        }, companyId, userId);

        return invoice;
    }
}

