using crm_backend.Data;
using crm_backend.Modules.Financial.DTOs;
using crm_backend.Modules.Opportunity;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Financial.Services;

public class QuoteService : IQuoteService
{
    private readonly CrmDbContext _context;

    public QuoteService(CrmDbContext context)
    {
        _context = context;
    }

    private async Task<string> GenerateQuoteNumberAsync(int companyId)
    {
        var year = DateTime.UtcNow.Year;
        var lastQuote = await _context.Quotes
            .Where(q => q.CompanyId == companyId && q.QuoteNumber.StartsWith($"QT-{year}-"))
            .OrderByDescending(q => q.QuoteNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastQuote != null)
        {
            var parts = lastQuote.QuoteNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"QT-{year}-{nextNumber:D3}";
    }

    public async Task<IEnumerable<QuoteDto>> GetAllQuotesAsync(int? companyId = null)
    {
        var query = _context.Quotes
            .Include(q => q.Company)
            .Include(q => q.Customer)
            .Include(q => q.Opportunity)
            .Include(q => q.CreatedByUser)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(q => q.CompanyId == companyId.Value);
        }

        var quotes = await query
            .Select(q => new QuoteDto
            {
                Id = q.Id,
                QuoteNumber = q.QuoteNumber,
                Title = q.Title,
                Description = q.Description,
                SubTotal = q.SubTotal,
                TaxAmount = q.TaxAmount,
                DiscountAmount = q.DiscountAmount,
                TotalAmount = q.TotalAmount,
                Currency = q.Currency,
                Status = q.Status,
                ValidUntil = q.ValidUntil,
                SentAt = q.SentAt,
                AcceptedAt = q.AcceptedAt,
                RejectedAt = q.RejectedAt,
                CustomerId = q.CustomerId,
                CustomerName = q.Customer.Name,
                OpportunityId = q.OpportunityId,
                OpportunityName = q.Opportunity != null ? q.Opportunity.Name : null,
                CreatedByUserId = q.CreatedByUserId,
                CreatedByUserName = q.CreatedByUser.Name,
                CompanyId = q.CompanyId,
                CompanyName = q.Company.Name,
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt
            })
            .ToListAsync();

        return quotes;
    }

    public async Task<QuoteDto?> GetQuoteByIdAsync(int id)
    {
        var quote = await _context.Quotes
            .Include(q => q.Company)
            .Include(q => q.Customer)
            .Include(q => q.Opportunity)
            .Include(q => q.CreatedByUser)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quote == null) return null;

        return new QuoteDto
        {
            Id = quote.Id,
            QuoteNumber = quote.QuoteNumber,
            Title = quote.Title,
            Description = quote.Description,
            SubTotal = quote.SubTotal,
            TaxAmount = quote.TaxAmount,
            DiscountAmount = quote.DiscountAmount,
            TotalAmount = quote.TotalAmount,
            Currency = quote.Currency,
            Status = quote.Status,
            ValidUntil = quote.ValidUntil,
            SentAt = quote.SentAt,
            AcceptedAt = quote.AcceptedAt,
            RejectedAt = quote.RejectedAt,
            CustomerId = quote.CustomerId,
            CustomerName = quote.Customer.Name,
            OpportunityId = quote.OpportunityId,
            OpportunityName = quote.Opportunity != null ? quote.Opportunity.Name : null,
            CreatedByUserId = quote.CreatedByUserId,
            CreatedByUserName = quote.CreatedByUser.Name,
            CompanyId = quote.CompanyId,
            CompanyName = quote.Company.Name,
            CreatedAt = quote.CreatedAt,
            UpdatedAt = quote.UpdatedAt
        };
    }

    public async Task<QuoteDto> CreateQuoteAsync(CreateQuoteDto dto)
    {
        var companyId = dto.CompanyId ?? throw new InvalidOperationException("Company ID is required");
        
        // Verify company exists
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId);
        if (!companyExists)
        {
            throw new InvalidOperationException($"Company with ID {companyId} does not exist.");
        }

        // Verify customer exists and belongs to company
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == dto.CustomerId);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer with ID {dto.CustomerId} does not exist.");
        }
        if (customer.CompanyId != companyId)
        {
            throw new InvalidOperationException("Customer does not belong to the specified company.");
        }

        // Verify opportunity exists and belongs to company (if provided)
        if (dto.OpportunityId.HasValue)
        {
            var opportunity = await _context.Opportunities.FirstOrDefaultAsync(o => o.Id == dto.OpportunityId.Value);
            if (opportunity == null)
            {
                throw new InvalidOperationException($"Opportunity with ID {dto.OpportunityId.Value} does not exist.");
            }
            if (opportunity.CompanyId != companyId)
            {
                throw new InvalidOperationException("Opportunity does not belong to the specified company.");
            }
            // Business rule: Quote can only be created from Open Opportunity
            if (opportunity.Status != OpportunityStatus.Open)
            {
                throw new InvalidOperationException($"Cannot create quote for opportunity with status {opportunity.Status}. Opportunity must be Open.");
            }
        }

        // Calculate totals from line items
        decimal subTotal = 0;
        foreach (var lineItem in dto.LineItems)
        {
            var product = await _context.Products.FindAsync(lineItem.ProductId);
            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID {lineItem.ProductId} does not exist.");
            }
            if (product.CompanyId != companyId)
            {
                throw new InvalidOperationException("Product does not belong to the specified company.");
            }

            var lineTotal = lineItem.Quantity * lineItem.UnitPrice * (1 - (lineItem.DiscountPercent ?? 0) / 100);
            subTotal += lineTotal;
        }

        var taxAmount = dto.TaxAmount ?? 0;
        var discountAmount = dto.DiscountAmount ?? 0;
        var totalAmount = subTotal + taxAmount - discountAmount;

        var quote = new Quote
        {
            QuoteNumber = await GenerateQuoteNumberAsync(companyId),
            Title = dto.Title,
            Description = dto.Description,
            SubTotal = subTotal,
            TaxAmount = taxAmount,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            Currency = dto.Currency,
            Status = QuoteStatus.Draft,
            ValidUntil = dto.ValidUntil,
            CustomerId = dto.CustomerId,
            OpportunityId = dto.OpportunityId,
            CreatedByUserId = dto.CreatedByUserId ?? throw new InvalidOperationException("CreatedByUserId is required"),
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync();

        // Add line items
        int order = 1;
        foreach (var lineItemDto in dto.LineItems)
        {
            var lineItem = new QuoteLineItem
            {
                QuoteId = quote.Id,
                ProductId = lineItemDto.ProductId,
                Description = lineItemDto.Description,
                Quantity = lineItemDto.Quantity,
                UnitPrice = lineItemDto.UnitPrice,
                DiscountPercent = lineItemDto.DiscountPercent,
                Order = order++
            };
            _context.QuoteLineItems.Add(lineItem);
        }

        await _context.SaveChangesAsync();

        // Load related entities for response
        await _context.Entry(quote).Reference(q => q.Company).LoadAsync();
        await _context.Entry(quote).Reference(q => q.Customer).LoadAsync();
        await _context.Entry(quote).Reference(q => q.CreatedByUser).LoadAsync();
        if (quote.OpportunityId.HasValue)
        {
            await _context.Entry(quote).Reference(q => q.Opportunity).LoadAsync();
        }

        return new QuoteDto
        {
            Id = quote.Id,
            QuoteNumber = quote.QuoteNumber,
            Title = quote.Title,
            Description = quote.Description,
            SubTotal = quote.SubTotal,
            TaxAmount = quote.TaxAmount,
            DiscountAmount = quote.DiscountAmount,
            TotalAmount = quote.TotalAmount,
            Currency = quote.Currency,
            Status = quote.Status,
            ValidUntil = quote.ValidUntil,
            SentAt = quote.SentAt,
            AcceptedAt = quote.AcceptedAt,
            RejectedAt = quote.RejectedAt,
            CustomerId = quote.CustomerId,
            CustomerName = quote.Customer.Name,
            OpportunityId = quote.OpportunityId,
            OpportunityName = quote.Opportunity != null ? quote.Opportunity.Name : null,
            CreatedByUserId = quote.CreatedByUserId,
            CreatedByUserName = quote.CreatedByUser.Name,
            CompanyId = quote.CompanyId,
            CompanyName = quote.Company.Name,
            CreatedAt = quote.CreatedAt,
            UpdatedAt = quote.UpdatedAt
        };
    }

    public async Task<QuoteDto?> UpdateQuoteAsync(int id, UpdateQuoteDto dto)
    {
        var quote = await _context.Quotes
            .Include(q => q.Company)
            .Include(q => q.Customer)
            .Include(q => q.Opportunity)
            .Include(q => q.CreatedByUser)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quote == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Title))
        {
            quote.Title = dto.Title;
        }

        if (dto.Description != null)
        {
            quote.Description = dto.Description;
        }

        if (dto.TaxAmount.HasValue)
        {
            quote.TaxAmount = dto.TaxAmount.Value;
            quote.TotalAmount = quote.SubTotal + quote.TaxAmount.Value - (quote.DiscountAmount ?? 0);
        }

        if (dto.DiscountAmount.HasValue)
        {
            quote.DiscountAmount = dto.DiscountAmount.Value;
            quote.TotalAmount = quote.SubTotal + (quote.TaxAmount ?? 0) - dto.DiscountAmount.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.Currency))
        {
            quote.Currency = dto.Currency;
        }

        if (dto.Status.HasValue)
        {
            quote.Status = dto.Status.Value;
            
            if (dto.Status.Value == QuoteStatus.Sent && quote.SentAt == null)
            {
                quote.SentAt = DateTime.UtcNow;
            }
            else if (dto.Status.Value == QuoteStatus.Accepted && quote.AcceptedAt == null)
            {
                quote.AcceptedAt = DateTime.UtcNow;
            }
            else if (dto.Status.Value == QuoteStatus.Rejected && quote.RejectedAt == null)
            {
                quote.RejectedAt = DateTime.UtcNow;
            }
        }

        if (dto.ValidUntil.HasValue)
        {
            quote.ValidUntil = dto.ValidUntil;
        }

        quote.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new QuoteDto
        {
            Id = quote.Id,
            QuoteNumber = quote.QuoteNumber,
            Title = quote.Title,
            Description = quote.Description,
            SubTotal = quote.SubTotal,
            TaxAmount = quote.TaxAmount,
            DiscountAmount = quote.DiscountAmount,
            TotalAmount = quote.TotalAmount,
            Currency = quote.Currency,
            Status = quote.Status,
            ValidUntil = quote.ValidUntil,
            SentAt = quote.SentAt,
            AcceptedAt = quote.AcceptedAt,
            RejectedAt = quote.RejectedAt,
            CustomerId = quote.CustomerId,
            CustomerName = quote.Customer.Name,
            OpportunityId = quote.OpportunityId,
            OpportunityName = quote.Opportunity != null ? quote.Opportunity.Name : null,
            CreatedByUserId = quote.CreatedByUserId,
            CreatedByUserName = quote.CreatedByUser.Name,
            CompanyId = quote.CompanyId,
            CompanyName = quote.Company.Name,
            CreatedAt = quote.CreatedAt,
            UpdatedAt = quote.UpdatedAt
        };
    }

    public async Task<bool> DeleteQuoteAsync(int id)
    {
        var quote = await _context.Quotes.FindAsync(id);
        if (quote == null) return false;

        // Delete line items first
        var lineItems = await _context.QuoteLineItems.Where(li => li.QuoteId == id).ToListAsync();
        _context.QuoteLineItems.RemoveRange(lineItems);

        _context.Quotes.Remove(quote);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<QuoteDto?> ConvertQuoteToInvoiceAsync(int quoteId)
    {
        var quote = await _context.Quotes
            .Include(q => q.LineItems)
            .Include(q => q.Customer)
            .Include(q => q.Opportunity)
            .FirstOrDefaultAsync(q => q.Id == quoteId);

        if (quote == null) return null;

        // Business rule: Quote must be Accepted before conversion
        if (quote.Status != QuoteStatus.Accepted)
            throw new InvalidOperationException($"Cannot convert quote with status {quote.Status}. Quote must be Accepted.");

        // Check if already converted
        if (quote.ConvertedToInvoiceId.HasValue)
            throw new InvalidOperationException("Quote has already been converted to an invoice.");

        // Mark quote as converted (invoice creation will be handled by caller or InvoiceService)
        quote.Status = QuoteStatus.Converted;
        quote.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetQuoteByIdAsync(quoteId);
    }

    public async Task<IEnumerable<QuoteDto>> GetQuotesByOpportunityAsync(int opportunityId, int companyId)
    {
        var opportunity = await _context.Opportunities
            .FirstOrDefaultAsync(o => o.Id == opportunityId && o.CompanyId == companyId);
        
        if (opportunity == null)
            throw new InvalidOperationException("Opportunity not found");

        var allQuotes = await GetAllQuotesAsync(companyId);
        return allQuotes.Where(q => q.OpportunityId == opportunityId);
    }
}

