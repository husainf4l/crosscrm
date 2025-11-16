using System.Security.Claims;
using crm_backend.Data;
using crm_backend.Modules.Company.DTOs;
using crm_backend.Modules.Company.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Company;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class CompanyResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<CompanyDto>> GetCompanies(
        [Service] ICompanyService companyService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.User == null)
        {
            throw new GraphQLException("User not authenticated");
        }

        var claimsPrincipal = httpContext.User;
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new GraphQLException("Invalid user token");
        }

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new GraphQLException("User not found");
        }

        // Get companies the user is associated with
        var userCompanies = await context.UserCompanies
            .Where(uc => uc.UserId == userId)
            .Select(uc => uc.CompanyId)
            .ToListAsync();

        if (!userCompanies.Any())
        {
            return new List<CompanyDto>();
        }

        var companies = await companyService.GetAllCompaniesAsync();
        return companies.Where(c => userCompanies.Contains(c.Id));
    }

    [Authorize]
    public async Task<CompanyDto?> GetCompany(
        int id,
        [Service] ICompanyService companyService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.User == null)
        {
            throw new GraphQLException("User not authenticated");
        }

        var claimsPrincipal = httpContext.User;
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new GraphQLException("Invalid user token");
        }

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new GraphQLException("User not found");
        }

        // Check if user is associated with this company
        var isAssociated = await context.UserCompanies
            .AnyAsync(uc => uc.UserId == userId && uc.CompanyId == id);

        if (!isAssociated)
        {
            return null; // Return null instead of throwing error for security
        }

        return await companyService.GetCompanyByIdAsync(id);
    }

    [Authorize]
    [UseProjection]
    public async Task<CompanyDto?> GetMyCompany(
        [Service] ICompanyService companyService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.User == null)
        {
            throw new GraphQLException("User not authenticated");
        }

        var claimsPrincipal = httpContext.User;
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new GraphQLException("Invalid user token");
        }

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new GraphQLException("User not found");
        }

        if (!user.CompanyId.HasValue)
        {
            return null; // User has no active company
        }

        return await companyService.GetCompanyByIdAsync(user.CompanyId.Value);
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class CompanyMutation
{
    public async Task<CompanyDto> CreateCompany(
        CreateCompanyDto input,
        [Service] ICompanyService companyService)
    {
        return await companyService.CreateCompanyAsync(input);
    }

    public async Task<CompanyDto?> UpdateCompany(
        int id,
        UpdateCompanyDto input,
        [Service] ICompanyService companyService)
    {
        return await companyService.UpdateCompanyAsync(id, input);
    }

    public async Task<bool> DeleteCompany(int id, [Service] ICompanyService companyService)
    {
        return await companyService.DeleteCompanyAsync(id);
    }

    [Authorize]
    public async Task<CompanyDto?> SwitchActiveCompany(
        int companyId,
        [Service] ICompanyService companyService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.User == null)
        {
            throw new GraphQLException("User not authenticated");
        }

        var claimsPrincipal = httpContext.User;
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new GraphQLException("Invalid user token");
        }

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new GraphQLException("User not found");
        }

        // Check if user is associated with the requested company
        var isAssociated = await context.UserCompanies
            .AnyAsync(uc => uc.UserId == userId && uc.CompanyId == companyId);

        if (!isAssociated)
        {
            throw new GraphQLException("User is not associated with this company");
        }

        // Update user's active company
        user.CompanyId = companyId;
        await context.SaveChangesAsync();

        // Return the new active company
        return await companyService.GetCompanyByIdAsync(companyId);
    }
}
