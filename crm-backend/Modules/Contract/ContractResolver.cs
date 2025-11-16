using crm_backend.Data;
using crm_backend.GraphQL;
using crm_backend.Modules.Contract.DTOs;
using crm_backend.Modules.Contract.Services;
using crm_backend.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace crm_backend.Modules.Contract;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class ContractResolver : BaseResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<ContractDto>> GetContracts(
        [Service] IContractService contractService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdOrNullAsync(httpContextAccessor, context);

        if (!companyId.HasValue)
        {
            return new List<ContractDto>();
        }

        return await contractService.GetAllContractsAsync(companyId.Value);
    }

    [Authorize]
    public async Task<ContractDto?> GetContract(
        int id,
        [Service] IContractService contractService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var contract = await contractService.GetContractByIdAsync(id);

        if (contract == null || contract.CompanyId != companyId)
        {
            return null;
        }

        return contract;
    }

    [Authorize]
    public async Task<IEnumerable<ContractDto>> GetExpiringContracts(
        int? daysAhead,
        [Service] IContractService contractService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        return await contractService.GetExpiringContractsAsync(companyId, daysAhead ?? 30);
    }

    [Authorize]
    public async Task<IEnumerable<ContractDto>> GetExpiredContracts(
        [Service] IContractService contractService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        return await contractService.GetExpiredContractsAsync(companyId);
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class ContractMutation : BaseResolver
{
    [Authorize]
    public async Task<ContractDto> CreateContract(
        CreateContractDto input,
        [Service] IContractService contractService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateContractDto> validator,
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

            var modifiedInput = new CreateContractDto
            {
                Name = input.Name,
                Description = input.Description,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                RenewalDate = input.RenewalDate,
                AutoRenew = input.AutoRenew,
                TotalValue = input.TotalValue,
                Currency = input.Currency,
                CustomerId = input.CustomerId,
                OpportunityId = input.OpportunityId,
                CompanyId = companyId,
                CreatedByUserId = userId,
                LineItems = input.LineItems
            };

            return await contractService.CreateContractAsync(modifiedInput);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to create contract");
        }
    }

    [Authorize]
    public async Task<ContractDto?> UpdateContract(
        int id,
        UpdateContractDto input,
        [Service] IContractService contractService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateContractDto> validator,
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

            var existingContract = await contractService.GetContractByIdAsync(id);
            if (existingContract == null || existingContract.CompanyId != companyId)
            {
                throw new GraphQLException("Contract not found or access denied");
            }

            return await contractService.UpdateContractAsync(id, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to update contract");
        }
    }

    [Authorize]
    public async Task<bool> DeleteContract(
        int id,
        [Service] IContractService contractService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingContract = await contractService.GetContractByIdAsync(id);
            if (existingContract == null || existingContract.CompanyId != companyId)
            {
                throw new GraphQLException("Contract not found or access denied");
            }

            return await contractService.DeleteContractAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to delete contract");
        }
    }

    [Authorize]
    public async Task<ContractDto> RenewContract(
        int id,
        [Service] IContractService contractService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingContract = await contractService.GetContractByIdAsync(id);
            if (existingContract == null || existingContract.CompanyId != companyId)
            {
                throw new GraphQLException("Contract not found or access denied");
            }

            return await contractService.RenewContractAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to renew contract");
        }
    }
}

