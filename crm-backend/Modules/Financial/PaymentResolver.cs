using HotChocolate;
using HotChocolate.Data;
using crm_backend.Modules.Financial.DTOs;
using crm_backend.Modules.Financial.Services;
using crm_backend.Data;
using crm_backend.GraphQL;
using crm_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using FluentValidation;

namespace crm_backend.Modules.Financial;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class PaymentResolver : BaseResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<PaymentDto>> GetPayments(
        [Service] IPaymentService paymentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdOrNullAsync(httpContextAccessor, context);
        
        if (!companyId.HasValue)
        {
            return new List<PaymentDto>();
        }

        return await paymentService.GetAllPaymentsAsync(companyId.Value);
    }

    [Authorize]
    public async Task<IEnumerable<PaymentDto>> GetPaymentsByInvoice(
        int invoiceId,
        [Service] IPaymentService paymentService,
        [Service] IInvoiceService invoiceService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

        // Verify invoice belongs to company
        var invoice = await invoiceService.GetInvoiceByIdAsync(invoiceId);
        if (invoice == null || invoice.CompanyId != companyId)
        {
            return new List<PaymentDto>();
        }

        return await paymentService.GetPaymentsByInvoiceAsync(invoiceId);
    }

    [Authorize]
    public async Task<PaymentDto?> GetPayment(
        int id,
        [Service] IPaymentService paymentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var payment = await paymentService.GetPaymentByIdAsync(id);

        if (payment == null || payment.CompanyId != companyId)
        {
            return null;
        }

        return payment;
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class PaymentMutation : BaseResolver
{
    [Authorize]
    public async Task<PaymentDto> CreatePayment(
        CreatePaymentDto input,
        [Service] IPaymentService paymentService,
        [Service] IInvoiceService invoiceService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreatePaymentDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new GraphQLException($"Validation failed: {errors}");
            }

            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            var userId = GetUserId(httpContextAccessor.HttpContext);

            // Verify invoice belongs to company
            var invoice = await invoiceService.GetInvoiceByIdAsync(input.InvoiceId);
            if (invoice == null || invoice.CompanyId != companyId)
            {
                throw new GraphQLException("Invoice not found or access denied");
            }

            var modifiedInput = new CreatePaymentDto
            {
                Amount = input.Amount,
                Currency = input.Currency,
                Method = input.Method,
                TransactionId = input.TransactionId,
                ReferenceNumber = input.ReferenceNumber,
                Notes = input.Notes,
                PaymentDate = input.PaymentDate,
                InvoiceId = input.InvoiceId,
                ReceivedByUserId = input.ReceivedByUserId ?? userId
            };

            return await paymentService.CreatePaymentAsync(modifiedInput);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to create payment");
        }
    }

    [Authorize]
    public async Task<bool> DeletePayment(
        int id,
        [Service] IPaymentService paymentService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingPayment = await paymentService.GetPaymentByIdAsync(id);
            if (existingPayment == null || existingPayment.CompanyId != companyId)
            {
                throw new GraphQLException("Payment not found or access denied");
            }

            return await paymentService.DeletePaymentAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to delete payment");
        }
    }
}

