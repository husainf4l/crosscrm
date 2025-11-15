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
public class QuoteResolver : BaseResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<QuoteDto>> GetQuotes(
        [Service] IQuoteService quoteService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdOrNullAsync(httpContextAccessor, context);
        
        if (!companyId.HasValue)
        {
            return new List<QuoteDto>();
        }

        return await quoteService.GetAllQuotesAsync(companyId.Value);
    }

    [Authorize]
    public async Task<QuoteDto?> GetQuote(
        int id,
        [Service] IQuoteService quoteService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var quote = await quoteService.GetQuoteByIdAsync(id);

        if (quote == null || quote.CompanyId != companyId)
        {
            return null;
        }

        return quote;
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class QuoteMutation : BaseResolver
{
    [Authorize]
    public async Task<QuoteDto> CreateQuote(
        CreateQuoteDto input,
        [Service] IQuoteService quoteService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateQuoteDto> validator,
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

            var modifiedInput = new CreateQuoteDto
            {
                Title = input.Title,
                Description = input.Description,
                TaxAmount = input.TaxAmount,
                DiscountAmount = input.DiscountAmount,
                Currency = input.Currency,
                ValidUntil = input.ValidUntil,
                CustomerId = input.CustomerId,
                OpportunityId = input.OpportunityId,
                CompanyId = companyId,
                CreatedByUserId = userId,
                LineItems = input.LineItems
            };

            return await quoteService.CreateQuoteAsync(modifiedInput);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to create quote");
        }
    }

    [Authorize]
    public async Task<QuoteDto?> UpdateQuote(
        int id,
        UpdateQuoteDto input,
        [Service] IQuoteService quoteService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateQuoteDto> validator,
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

            var existingQuote = await quoteService.GetQuoteByIdAsync(id);
            if (existingQuote == null || existingQuote.CompanyId != companyId)
            {
                throw new GraphQLException("Quote not found or access denied");
            }

            return await quoteService.UpdateQuoteAsync(id, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to update quote");
        }
    }

    [Authorize]
    public async Task<bool> DeleteQuote(
        int id,
        [Service] IQuoteService quoteService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingQuote = await quoteService.GetQuoteByIdAsync(id);
            if (existingQuote == null || existingQuote.CompanyId != companyId)
            {
                throw new GraphQLException("Quote not found or access denied");
            }

            return await quoteService.DeleteQuoteAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to delete quote");
        }
    }

    [Authorize]
    public async Task<QuoteDto?> ConvertQuoteToInvoice(
        int id,
        [Service] IQuoteService quoteService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingQuote = await quoteService.GetQuoteByIdAsync(id);
            if (existingQuote == null || existingQuote.CompanyId != companyId)
            {
                throw new GraphQLException("Quote not found or access denied");
            }

            return await quoteService.ConvertQuoteToInvoiceAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to convert quote to invoice");
        }
    }
}

