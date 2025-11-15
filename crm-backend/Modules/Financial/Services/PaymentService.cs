using crm_backend.Data;
using crm_backend.Modules.Collaboration.Services;
using crm_backend.Modules.Financial.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Financial.Services;

public class PaymentService : IPaymentService
{
    private readonly CrmDbContext _context;
    private readonly IActivityTimelineService _activityTimelineService;

    public PaymentService(CrmDbContext context, IActivityTimelineService activityTimelineService)
    {
        _context = context;
        _activityTimelineService = activityTimelineService;
    }

    private async Task<string> GeneratePaymentNumberAsync(int companyId)
    {
        var year = DateTime.UtcNow.Year;
        var lastPayment = await _context.Payments
            .Where(p => p.CompanyId == companyId && p.PaymentNumber.StartsWith($"PAY-{year}-"))
            .OrderByDescending(p => p.PaymentNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastPayment != null)
        {
            var parts = lastPayment.PaymentNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"PAY-{year}-{nextNumber:D3}";
    }

    public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync(int? companyId = null)
    {
        var query = _context.Payments
            .Include(p => p.Company)
            .Include(p => p.Customer)
            .Include(p => p.Invoice)
            .Include(p => p.ReceivedByUser)
            .Include(p => p.CreatedByUser)
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(p => p.CompanyId == companyId.Value);
        }

        var payments = await query
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                PaymentNumber = p.PaymentNumber,
                Amount = p.Amount,
                Currency = p.Currency,
                Method = p.Method,
                TransactionId = p.TransactionId,
                ReferenceNumber = p.ReferenceNumber,
                Notes = p.Notes,
                PaymentDate = p.PaymentDate,
                InvoiceId = p.InvoiceId,
                InvoiceNumber = p.Invoice.InvoiceNumber,
                CustomerId = p.CustomerId,
                CustomerName = p.Customer.Name,
                ReceivedByUserId = p.ReceivedByUserId,
                ReceivedByUserName = p.ReceivedByUser != null ? p.ReceivedByUser.Name : null,
                CreatedByUserId = p.CreatedByUserId,
                CreatedByUserName = p.CreatedByUser != null ? p.CreatedByUser.Name : null,
                CompanyId = p.CompanyId,
                CompanyName = p.Company.Name,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return payments;
    }

    public async Task<IEnumerable<PaymentDto>> GetPaymentsByInvoiceAsync(int invoiceId)
    {
        var payments = await _context.Payments
            .Include(p => p.Company)
            .Include(p => p.Customer)
            .Include(p => p.Invoice)
            .Include(p => p.ReceivedByUser)
            .Include(p => p.CreatedByUser)
            .Where(p => p.InvoiceId == invoiceId)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                PaymentNumber = p.PaymentNumber,
                Amount = p.Amount,
                Currency = p.Currency,
                Method = p.Method,
                TransactionId = p.TransactionId,
                ReferenceNumber = p.ReferenceNumber,
                Notes = p.Notes,
                PaymentDate = p.PaymentDate,
                InvoiceId = p.InvoiceId,
                InvoiceNumber = p.Invoice.InvoiceNumber,
                CustomerId = p.CustomerId,
                CustomerName = p.Customer.Name,
                ReceivedByUserId = p.ReceivedByUserId,
                ReceivedByUserName = p.ReceivedByUser != null ? p.ReceivedByUser.Name : null,
                CreatedByUserId = p.CreatedByUserId,
                CreatedByUserName = p.CreatedByUser != null ? p.CreatedByUser.Name : null,
                CompanyId = p.CompanyId,
                CompanyName = p.Company.Name,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return payments;
    }

    public async Task<PaymentDto?> GetPaymentByIdAsync(int id)
    {
        var payment = await _context.Payments
            .Include(p => p.Company)
            .Include(p => p.Customer)
            .Include(p => p.Invoice)
            .Include(p => p.ReceivedByUser)
            .Include(p => p.CreatedByUser)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment == null) return null;

        return new PaymentDto
        {
            Id = payment.Id,
            PaymentNumber = payment.PaymentNumber,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Method = payment.Method,
            TransactionId = payment.TransactionId,
            ReferenceNumber = payment.ReferenceNumber,
            Notes = payment.Notes,
            PaymentDate = payment.PaymentDate,
            InvoiceId = payment.InvoiceId,
            InvoiceNumber = payment.Invoice.InvoiceNumber,
            CustomerId = payment.CustomerId,
            CustomerName = payment.Customer.Name,
            ReceivedByUserId = payment.ReceivedByUserId,
            ReceivedByUserName = payment.ReceivedByUser != null ? payment.ReceivedByUser.Name : null,
            CreatedByUserId = payment.CreatedByUserId,
            CreatedByUserName = payment.CreatedByUser?.Name,
            CompanyId = payment.CompanyId,
            CompanyName = payment.Company.Name,
            CreatedAt = payment.CreatedAt
        };
    }

    public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto)
    {
        // Verify invoice exists
        var invoice = await _context.Invoices
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.Id == dto.InvoiceId);
        
        if (invoice == null)
        {
            throw new InvalidOperationException($"Invoice with ID {dto.InvoiceId} does not exist.");
        }

        var companyId = invoice.CompanyId;

        // Customer ID is automatically set from invoice

        // Verify received by user belongs to company (if provided)
        if (dto.ReceivedByUserId.HasValue)
        {
            var userCompany = await _context.UserCompanies
                .AnyAsync(uc => uc.UserId == dto.ReceivedByUserId.Value && uc.CompanyId == companyId);
            if (!userCompany)
            {
                throw new InvalidOperationException("User does not belong to the invoice's company.");
            }
        }

        // Verify payment amount doesn't exceed invoice balance
        var totalPaid = await _context.Payments
            .Where(p => p.InvoiceId == dto.InvoiceId)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        if (totalPaid + dto.Amount > invoice.TotalAmount)
        {
            throw new InvalidOperationException("Payment amount exceeds the invoice balance.");
        }

        var payment = new Payment
        {
            PaymentNumber = await GeneratePaymentNumberAsync(companyId),
            Amount = dto.Amount,
            Currency = dto.Currency,
            Method = dto.Method,
            TransactionId = dto.TransactionId,
            ReferenceNumber = dto.ReferenceNumber,
            Notes = dto.Notes,
            PaymentDate = dto.PaymentDate ?? DateTime.UtcNow,
            InvoiceId = dto.InvoiceId,
            CustomerId = invoice.CustomerId, // Set from invoice
            ReceivedByUserId = dto.ReceivedByUserId,
            CreatedByUserId = dto.CreatedByUserId ?? dto.ReceivedByUserId ?? throw new InvalidOperationException("CreatedByUserId or ReceivedByUserId is required"),
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        
        // Update invoice paid amount and status
        invoice.PaidAmount += dto.Amount;
        invoice.UpdatedAt = DateTime.UtcNow;
        
        // Auto-update invoice status based on payment
        if (invoice.PaidAmount >= invoice.TotalAmount)
        {
            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.UtcNow;
        }
        else if (invoice.PaidAmount > 0)
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }
        
        await _context.SaveChangesAsync();
        
        // Update invoice status (triggers any additional workflow)
        var invoiceService = new InvoiceService(_context);
        await invoiceService.UpdateInvoiceStatusAsync(invoice.Id);

        // Log activity timeline entry
        await _activityTimelineService.LogActivityAsync(new Collaboration.DTOs.CreateActivityTimelineDto
        {
            EntityType = "Payment",
            EntityId = payment.Id,
            Type = "Created",
            Description = $"Payment of {dto.Amount} {dto.Currency} received for Invoice {invoice.InvoiceNumber}"
        }, companyId, payment.CreatedByUserId);

        // Log activity for invoice
        await _activityTimelineService.LogActivityAsync(new Collaboration.DTOs.CreateActivityTimelineDto
        {
            EntityType = "Invoice",
            EntityId = invoice.Id,
            Type = "PaymentReceived",
            Description = $"Payment of {dto.Amount} {dto.Currency} received. Balance: {invoice.BalanceAmount} {invoice.Currency}"
        }, companyId, payment.CreatedByUserId);

        // Update Opportunity if invoice is linked to one
        if (invoice.OpportunityId.HasValue)
        {
            var opportunity = await _context.Opportunities
                .FirstOrDefaultAsync(o => o.Id == invoice.OpportunityId.Value);
            
            if (opportunity != null)
            {
                await _activityTimelineService.LogActivityAsync(new Collaboration.DTOs.CreateActivityTimelineDto
                {
                    EntityType = "Opportunity",
                    EntityId = opportunity.Id,
                    Type = "PaymentReceived",
                    Description = $"Payment of {dto.Amount} {dto.Currency} received for Invoice {invoice.InvoiceNumber}"
                }, companyId, payment.CreatedByUserId);
            }
        }

        // Load related entities for response
        await _context.Entry(payment).Reference(p => p.Company).LoadAsync();
        await _context.Entry(payment).Reference(p => p.Customer).LoadAsync();
        await _context.Entry(payment).Reference(p => p.Invoice).LoadAsync();
        await _context.Entry(payment).Reference(p => p.CreatedByUser).LoadAsync();
        if (payment.ReceivedByUserId.HasValue)
        {
            await _context.Entry(payment).Reference(p => p.ReceivedByUser).LoadAsync();
        }

        return new PaymentDto
        {
            Id = payment.Id,
            PaymentNumber = payment.PaymentNumber,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Method = payment.Method,
            TransactionId = payment.TransactionId,
            ReferenceNumber = payment.ReferenceNumber,
            Notes = payment.Notes,
            PaymentDate = payment.PaymentDate,
            InvoiceId = payment.InvoiceId,
            InvoiceNumber = payment.Invoice.InvoiceNumber,
            CustomerId = payment.CustomerId,
            CustomerName = payment.Customer.Name,
            ReceivedByUserId = payment.ReceivedByUserId,
            ReceivedByUserName = payment.ReceivedByUser != null ? payment.ReceivedByUser.Name : null,
            CompanyId = payment.CompanyId,
            CompanyName = payment.Company.Name,
            CreatedAt = payment.CreatedAt
        };
    }

    public async Task<bool> DeletePaymentAsync(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null) return false;

        var invoiceId = payment.InvoiceId;

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync();

        // Update invoice status after payment deletion
        await UpdateInvoiceStatusAsync(invoiceId);

        return true;
    }

    private async Task UpdateInvoiceStatusAsync(int invoiceId)
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

