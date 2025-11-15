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
public class InvoiceResolver : BaseResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<InvoiceDto>> GetInvoices(
        [Service] IInvoiceService invoiceService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdOrNullAsync(httpContextAccessor, context);
        
        if (!companyId.HasValue)
        {
            return new List<InvoiceDto>();
        }

        return await invoiceService.GetAllInvoicesAsync(companyId.Value);
    }

    [Authorize]
    public async Task<InvoiceDto?> GetInvoice(
        int id,
        [Service] IInvoiceService invoiceService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var invoice = await invoiceService.GetInvoiceByIdAsync(id);

        if (invoice == null || invoice.CompanyId != companyId)
        {
            return null;
        }

        return invoice;
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class InvoiceMutation : BaseResolver
{
    [Authorize]
    public async Task<InvoiceDto> CreateInvoice(
        CreateInvoiceDto input,
        [Service] IInvoiceService invoiceService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateInvoiceDto> validator,
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

            var modifiedInput = new CreateInvoiceDto
            {
                Title = input.Title,
                Description = input.Description,
                TaxAmount = input.TaxAmount,
                DiscountAmount = input.DiscountAmount,
                Currency = input.Currency,
                InvoiceDate = input.InvoiceDate,
                DueDate = input.DueDate,
                CustomerId = input.CustomerId,
                QuoteId = input.QuoteId,
                OpportunityId = input.OpportunityId,
                CompanyId = companyId,
                CreatedByUserId = userId,
                LineItems = input.LineItems
            };

            return await invoiceService.CreateInvoiceAsync(modifiedInput);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to create invoice");
        }
    }

    [Authorize]
    public async Task<InvoiceDto?> UpdateInvoice(
        int id,
        UpdateInvoiceDto input,
        [Service] IInvoiceService invoiceService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateInvoiceDto> validator,
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

            var existingInvoice = await invoiceService.GetInvoiceByIdAsync(id);
            if (existingInvoice == null || existingInvoice.CompanyId != companyId)
            {
                throw new GraphQLException("Invoice not found or access denied");
            }

            return await invoiceService.UpdateInvoiceAsync(id, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to update invoice");
        }
    }

    [Authorize]
    public async Task<bool> DeleteInvoice(
        int id,
        [Service] IInvoiceService invoiceService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingInvoice = await invoiceService.GetInvoiceByIdAsync(id);
            if (existingInvoice == null || existingInvoice.CompanyId != companyId)
            {
                throw new GraphQLException("Invoice not found or access denied");
            }

            return await invoiceService.DeleteInvoiceAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to delete invoice");
        }
    }
}

