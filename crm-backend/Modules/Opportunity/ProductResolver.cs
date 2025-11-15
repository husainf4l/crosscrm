using HotChocolate;
using HotChocolate.Data;
using crm_backend.Modules.Opportunity.DTOs;
using crm_backend.Modules.Opportunity.Services;
using crm_backend.Data;
using crm_backend.GraphQL;
using crm_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using FluentValidation;

namespace crm_backend.Modules.Opportunity;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class ProductResolver : BaseResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<ProductDto>> GetProducts(
        [Service] IProductService productService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdOrNullAsync(httpContextAccessor, context);
        
        if (!companyId.HasValue)
        {
            return new List<ProductDto>();
        }

        return await productService.GetAllProductsAsync(companyId.Value);
    }

    [Authorize]
    public async Task<ProductDto?> GetProduct(
        int id,
        [Service] IProductService productService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var product = await productService.GetProductByIdAsync(id);

        if (product == null || product.CompanyId != companyId)
        {
            return null;
        }

        return product;
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class ProductMutation : BaseResolver
{
    [Authorize]
    public async Task<ProductDto> CreateProduct(
        CreateProductDto input,
        [Service] IProductService productService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateProductDto> validator,
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

            var modifiedInput = new CreateProductDto
            {
                Name = input.Name,
                Description = input.Description,
                SKU = input.SKU,
                Price = input.Price,
                Currency = input.Currency,
                Cost = input.Cost,
                Type = input.Type,
                Unit = input.Unit,
                IsActive = input.IsActive,
                IsTaxable = input.IsTaxable,
                CompanyId = companyId
            };

            return await productService.CreateProductAsync(modifiedInput);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to create product");
        }
    }

    [Authorize]
    public async Task<ProductDto?> UpdateProduct(
        int id,
        UpdateProductDto input,
        [Service] IProductService productService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateProductDto> validator,
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

            var existingProduct = await productService.GetProductByIdAsync(id);
            if (existingProduct == null || existingProduct.CompanyId != companyId)
            {
                throw new GraphQLException("Product not found or access denied");
            }

            return await productService.UpdateProductAsync(id, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to update product");
        }
    }

    [Authorize]
    public async Task<bool> DeleteProduct(
        int id,
        [Service] IProductService productService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingProduct = await productService.GetProductByIdAsync(id);
            if (existingProduct == null || existingProduct.CompanyId != companyId)
            {
                throw new GraphQLException("Product not found or access denied");
            }

            return await productService.DeleteProductAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to delete product");
        }
    }
}

