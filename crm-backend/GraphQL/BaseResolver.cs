using HotChocolate;
using System.Security.Claims;
using crm_backend.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.GraphQL;

/// <summary>
/// Base resolver class that provides common authorization and user context functionality
/// </summary>
public abstract class BaseResolver
{
    /// <summary>
    /// Gets the authenticated user ID from the JWT token claims
    /// </summary>
    protected static int GetUserId(HttpContext? httpContext)
    {
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

        return userId;
    }

    /// <summary>
    /// Gets the authenticated user with their company information
    /// </summary>
    protected static async Task<Modules.User.User> GetAuthenticatedUserAsync(
        IHttpContextAccessor httpContextAccessor,
        CrmDbContext context)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var userId = GetUserId(httpContext);

        var user = await context.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new GraphQLException("User not found");
        }

        return user;
    }

    /// <summary>
    /// Gets the active company ID for the authenticated user
    /// </summary>
    protected static async Task<int> GetActiveCompanyIdAsync(
        IHttpContextAccessor httpContextAccessor,
        CrmDbContext context)
    {
        var user = await GetAuthenticatedUserAsync(httpContextAccessor, context);

        if (!user.CompanyId.HasValue)
        {
            throw new GraphQLException("User has no active company");
        }

        return user.CompanyId.Value;
    }

    /// <summary>
    /// Gets the active company ID for the authenticated user, or null if none is set
    /// </summary>
    protected static async Task<int?> GetActiveCompanyIdOrNullAsync(
        IHttpContextAccessor httpContextAccessor,
        CrmDbContext context)
    {
        var user = await GetAuthenticatedUserAsync(httpContextAccessor, context);
        return user.CompanyId;
    }

    /// <summary>
    /// Verifies that a customer belongs to the user's active company
    /// </summary>
    protected static async Task<bool> VerifyCustomerAccessAsync(
        int customerId,
        int companyId,
        CrmDbContext context)
    {
        var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId);

        return customer != null && customer.CompanyId == companyId;
    }
}

