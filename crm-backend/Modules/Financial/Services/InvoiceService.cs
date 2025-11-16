using crm_backend.Data;
using crm_backend.Modules.Financial.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Financial.Services;

public class InvoiceService : IInvoiceService
{
    private readonly CrmDbContext _context;

    public InvoiceService(CrmDbContext context)
    {
        _context = context;
    }

    private async Task<string> GenerateInvoiceNumberAsync(int companyId)
    {
        var year = DateTime.UtcNow.Year;
        var lastInvoice = await _context.Invoices
            .Where(i => i.CompanyId == companyId && i.InvoiceNumber.StartsWith($"INV-{year}-"))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastInvoice != null)
        {
            var parts = lastInvoice.InvoiceNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"INV-{year}-{nextNumber:D3}";
    }

    public async Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync(int? companyId = null)
    {
        var query = _context.Invoices
            .Include(i => i.Company)
            .Include(i => i.Customer)
            .Include(i => i.Quote)
            .Include(i => i.Opportunity)
            .Include(i => i.CreatedByUser)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(i => i.CompanyId == companyId.Value);
        }

        var invoices = await query
            .Select(i => new InvoiceDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                Title = i.Title,
                Description = i.Description,
                SubTotal = i.SubTotal,
                TaxAmount = i.TaxAmount,
                DiscountAmount = i.DiscountAmount,
                TotalAmount = i.TotalAmount,
                PaidAmount = i.PaidAmount,
                BalanceAmount = i.BalanceAmount,
                Currency = i.Currency,
                InvoiceDate = i.InvoiceDate,
                DueDate = i.DueDate,
                PaidAt = i.PaidAt,
                Status = i.Status,
                CustomerId = i.CustomerId,
                CustomerName = i.Customer.Name,
                QuoteId = i.QuoteId,
                QuoteNumber = i.Quote != null ? i.Quote.QuoteNumber : null,
                OpportunityId = i.OpportunityId,
                OpportunityName = i.Opportunity != null ? i.Opportunity.Name : null,
                CreatedByUserId = i.CreatedByUserId,
                CreatedByUserName = i.CreatedByUser.Name,
                CompanyId = i.CompanyId,
                CompanyName = i.Company.Name,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            })
            .ToListAsync();

        return invoices;
    }

    public async Task<InvoiceDto?> GetInvoiceByIdAsync(int id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Company)
            .Include(i => i.Customer)
            .Include(i => i.Quote)
            .Include(i => i.Opportunity)
            .Include(i => i.CreatedByUser)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null) return null;

        return new InvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            Title = invoice.Title,
            Description = invoice.Description,
            SubTotal = invoice.SubTotal,
            TaxAmount = invoice.TaxAmount,
            DiscountAmount = invoice.DiscountAmount,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            BalanceAmount = invoice.BalanceAmount,
            Currency = invoice.Currency,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            PaidAt = invoice.PaidAt,
            Status = invoice.Status,
            CustomerId = invoice.CustomerId,
            CustomerName = invoice.Customer.Name,
            QuoteId = invoice.QuoteId,
            QuoteNumber = invoice.Quote != null ? invoice.Quote.QuoteNumber : null,
            OpportunityId = invoice.OpportunityId,
            OpportunityName = invoice.Opportunity != null ? invoice.Opportunity.Name : null,
            CreatedByUserId = invoice.CreatedByUserId,
            CreatedByUserName = invoice.CreatedByUser.Name,
            CompanyId = invoice.CompanyId,
            CompanyName = invoice.Company.Name,
            CreatedAt = invoice.CreatedAt,
            UpdatedAt = invoice.UpdatedAt
        };
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceDto dto)
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

        // Verify quote exists and belongs to company (if provided)
        if (dto.QuoteId.HasValue)
        {
            var quote = await _context.Quotes.FirstOrDefaultAsync(q => q.Id == dto.QuoteId.Value);
            if (quote == null)
            {
                throw new InvalidOperationException($"Quote with ID {dto.QuoteId.Value} does not exist.");
            }
            if (quote.CompanyId != companyId)
            {
                throw new InvalidOperationException("Quote does not belong to the specified company.");
            }
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

        var invoice = new Invoice
        {
            InvoiceNumber = await GenerateInvoiceNumberAsync(companyId),
            Title = dto.Title,
            Description = dto.Description,
            SubTotal = subTotal,
            TaxAmount = taxAmount,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            PaidAmount = 0,
            Currency = dto.Currency,
            InvoiceDate = dto.InvoiceDate ?? DateTime.UtcNow,
            DueDate = dto.DueDate,
            Status = InvoiceStatus.Draft,
            CustomerId = dto.CustomerId,
            QuoteId = dto.QuoteId,
            OpportunityId = dto.OpportunityId,
            CreatedByUserId = dto.CreatedByUserId ?? throw new InvalidOperationException("CreatedByUserId is required"),
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Add line items
        int order = 1;
        foreach (var lineItemDto in dto.LineItems)
        {
            var lineItem = new InvoiceLineItem
            {
                InvoiceId = invoice.Id,
                ProductId = lineItemDto.ProductId,
                Description = lineItemDto.Description,
                Quantity = lineItemDto.Quantity,
                UnitPrice = lineItemDto.UnitPrice,
                DiscountPercent = lineItemDto.DiscountPercent,
                Order = order++
            };
            _context.InvoiceLineItems.Add(lineItem);
        }

        await _context.SaveChangesAsync();

        // Load related entities for response
        await _context.Entry(invoice).Reference(i => i.Company).LoadAsync();
        await _context.Entry(invoice).Reference(i => i.Customer).LoadAsync();
        await _context.Entry(invoice).Reference(i => i.CreatedByUser).LoadAsync();
        if (invoice.QuoteId.HasValue)
        {
            await _context.Entry(invoice).Reference(i => i.Quote).LoadAsync();
        }
        if (invoice.OpportunityId.HasValue)
        {
            await _context.Entry(invoice).Reference(i => i.Opportunity).LoadAsync();
        }

        return new InvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            Title = invoice.Title,
            Description = invoice.Description,
            SubTotal = invoice.SubTotal,
            TaxAmount = invoice.TaxAmount,
            DiscountAmount = invoice.DiscountAmount,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            BalanceAmount = invoice.BalanceAmount,
            Currency = invoice.Currency,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            PaidAt = invoice.PaidAt,
            Status = invoice.Status,
            CustomerId = invoice.CustomerId,
            CustomerName = invoice.Customer.Name,
            QuoteId = invoice.QuoteId,
            QuoteNumber = invoice.Quote != null ? invoice.Quote.QuoteNumber : null,
            OpportunityId = invoice.OpportunityId,
            OpportunityName = invoice.Opportunity != null ? invoice.Opportunity.Name : null,
            CreatedByUserId = invoice.CreatedByUserId,
            CreatedByUserName = invoice.CreatedByUser.Name,
            CompanyId = invoice.CompanyId,
            CompanyName = invoice.Company.Name,
            CreatedAt = invoice.CreatedAt,
            UpdatedAt = invoice.UpdatedAt
        };
    }

    public async Task<InvoiceDto?> UpdateInvoiceAsync(int id, UpdateInvoiceDto dto)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Company)
            .Include(i => i.Customer)
            .Include(i => i.Quote)
            .Include(i => i.Opportunity)
            .Include(i => i.CreatedByUser)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Title))
        {
            invoice.Title = dto.Title;
        }

        if (dto.Description != null)
        {
            invoice.Description = dto.Description;
        }

        if (dto.TaxAmount.HasValue)
        {
            invoice.TaxAmount = dto.TaxAmount.Value;
            invoice.TotalAmount = invoice.SubTotal + invoice.TaxAmount.Value - (invoice.DiscountAmount ?? 0);
        }

        if (dto.DiscountAmount.HasValue)
        {
            invoice.DiscountAmount = dto.DiscountAmount.Value;
            invoice.TotalAmount = invoice.SubTotal + (invoice.TaxAmount ?? 0) - dto.DiscountAmount.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.Currency))
        {
            invoice.Currency = dto.Currency;
        }

        if (dto.InvoiceDate.HasValue)
        {
            invoice.InvoiceDate = dto.InvoiceDate.Value;
        }

        if (dto.DueDate.HasValue)
        {
            invoice.DueDate = dto.DueDate.Value;
        }

        if (dto.Status.HasValue)
        {
            invoice.Status = dto.Status.Value;
        }

        invoice.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Update status based on payments
        await UpdateInvoiceStatusAsync(id);

        return await GetInvoiceByIdAsync(id);
    }

    public async Task<bool> DeleteInvoiceAsync(int id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null) return false;

        // Check if invoice has payments
        var hasPayments = await _context.Payments.AnyAsync(p => p.InvoiceId == id);
        if (hasPayments)
        {
            throw new InvalidOperationException("Cannot delete invoice that has associated payments.");
        }

        // Delete line items first
        var lineItems = await _context.InvoiceLineItems.Where(li => li.InvoiceId == id).ToListAsync();
        _context.InvoiceLineItems.RemoveRange(lineItems);

        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task UpdateInvoiceStatusAsync(int invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null) return;

        // Calculate total paid amount
        var totalPaid = invoice.Payments.Sum(p => p.Amount);
        invoice.PaidAmount = totalPaid;

        // Auto-update status based on payments and due date
        if (totalPaid >= invoice.TotalAmount)
        {
            invoice.Status = InvoiceStatus.Paid;
            if (invoice.PaidAt == null)
                invoice.PaidAt = DateTime.UtcNow;
        }
        else if (totalPaid > 0)
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }
        else if (invoice.Status == InvoiceStatus.Sent && invoice.DueDate < DateTime.UtcNow)
        {
            invoice.Status = InvoiceStatus.Overdue;
        }

        invoice.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateInvoiceStatusAsync_OLD(int invoiceId)
    {
        var invoice = await _context.Invoices.FindAsync(invoiceId);
        if (invoice == null) return;

        // Calculate total paid amount
        var totalPaid = await _context.Payments
            .Where(p => p.InvoiceId == invoiceId)
            .SumAsync(p => p.Amount);

        invoice.PaidAmount = totalPaid;

        // Update status based on paid amount and due date
        if (invoice.PaidAmount >= invoice.TotalAmount)
        {
            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.UtcNow;
        }
        else if (invoice.PaidAmount > 0)
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }
        else if (invoice.DueDate < DateTime.UtcNow.Date && invoice.Status != InvoiceStatus.Cancelled)
        {
            invoice.Status = InvoiceStatus.Overdue;
        }
        else if (invoice.Status == InvoiceStatus.Draft)
        {
            invoice.Status = InvoiceStatus.Sent;
        }

        invoice.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}

