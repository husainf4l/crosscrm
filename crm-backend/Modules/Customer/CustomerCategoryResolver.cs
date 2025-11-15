using crm_backend.Modules.Customer.DTOs;
using crm_backend.Modules.Customer.Services;

namespace crm_backend.Modules.Customer;

[ExtendObjectType("Mutation")]
public class CustomerCategoryMutation
{
    public async Task<CustomerCategoryDto> CreateCustomerCategory(
        CreateCustomerCategoryDto input,
        [Service] ICustomerCategoryService categoryService,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = GetUserIdFromContext(httpContextAccessor);
        return await categoryService.CreateCategoryAsync(input, userId);
    }

    public async Task<CustomerCategoryDto?> UpdateCustomerCategory(
        int categoryId,
        UpdateCustomerCategoryDto input,
        [Service] ICustomerCategoryService categoryService,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = GetUserIdFromContext(httpContextAccessor);
        return await categoryService.UpdateCategoryAsync(categoryId, input, userId);
    }

    public async Task<bool> DeleteCustomerCategory(
        int categoryId,
        [Service] ICustomerCategoryService categoryService,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = GetUserIdFromContext(httpContextAccessor);
        return await categoryService.DeleteCategoryAsync(categoryId, userId);
    }

    public async Task<CustomerCategoryMappingDto> AssignCustomerToCategory(
        AssignCustomerToCategoryDto input,
        [Service] ICustomerCategoryService categoryService,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = GetUserIdFromContext(httpContextAccessor);
        return await categoryService.AssignCustomerToCategoryAsync(input, userId);
    }

    public async Task<bool> RemoveCustomerFromCategory(
        int customerId,
        int categoryId,
        [Service] ICustomerCategoryService categoryService,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = GetUserIdFromContext(httpContextAccessor);
        return await categoryService.RemoveCustomerFromCategoryAsync(customerId, categoryId, userId);
    }

    [ExtendObjectType("Query")]
    public class CustomerCategoryQuery
    {
        public async Task<IEnumerable<CustomerCategoryDto>> GetCustomerCategories(
            [Service] ICustomerCategoryService categoryService,
            [Service] IHttpContextAccessor httpContextAccessor)
        {
            var userId = GetUserIdFromContext(httpContextAccessor);
            return await categoryService.GetCategoriesAsync(userId);
        }

        public async Task<CustomerCategoryDto?> GetCustomerCategory(
            int categoryId,
            [Service] ICustomerCategoryService categoryService,
            [Service] IHttpContextAccessor httpContextAccessor)
        {
            var userId = GetUserIdFromContext(httpContextAccessor);
            return await categoryService.GetCategoryAsync(categoryId, userId);
        }

        public async Task<IEnumerable<CustomerCategoryDto>> GetCustomerCategoriesByCustomer(
            int customerId,
            [Service] ICustomerCategoryService categoryService,
            [Service] IHttpContextAccessor httpContextAccessor)
        {
            var userId = GetUserIdFromContext(httpContextAccessor);
            return await categoryService.GetCustomerCategoriesAsync(customerId, userId);
        }

        public async Task<IEnumerable<CustomerDto>> GetCustomersByCategory(
            int categoryId,
            [Service] ICustomerCategoryService categoryService,
            [Service] IHttpContextAccessor httpContextAccessor)
        {
            var userId = GetUserIdFromContext(httpContextAccessor);
            return await categoryService.GetCustomersByCategoryAsync(categoryId, userId);
        }
    }

    private static int GetUserIdFromContext(IHttpContextAccessor httpContextAccessor)
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            throw new GraphQLException("Unauthorized");
        return userId;
    }
}