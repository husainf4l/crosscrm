using crm_backend.Modules.Customer.DTOs;
using crm_backend.Modules.Customer.Services;

namespace crm_backend.Modules.Customer;

[ExtendObjectType("Mutation")]
public class CustomerPreferenceMutation
{
    public async Task<CustomerPreferenceDto> CreateCustomerPreference(
        CreateCustomerPreferenceDto input,
        [Service] ICustomerPreferenceService preferenceService,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = GetUserIdFromContext(httpContextAccessor);
        return await preferenceService.CreatePreferenceAsync(input, userId);
    }

    public async Task<CustomerPreferenceDto?> UpdateCustomerPreference(
        int preferenceId,
        UpdateCustomerPreferenceDto input,
        [Service] ICustomerPreferenceService preferenceService,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = GetUserIdFromContext(httpContextAccessor);
        return await preferenceService.UpdatePreferenceAsync(preferenceId, input, userId);
    }

    public async Task<bool> DeleteCustomerPreference(
        int preferenceId,
        [Service] ICustomerPreferenceService preferenceService,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = GetUserIdFromContext(httpContextAccessor);
        return await preferenceService.DeletePreferenceAsync(preferenceId, userId);
    }

    [ExtendObjectType("Query")]
    public class CustomerPreferenceQuery
    {
        public async Task<IEnumerable<CustomerPreferenceDto>> GetCustomerPreferences(
            int customerId,
            [Service] ICustomerPreferenceService preferenceService,
            [Service] IHttpContextAccessor httpContextAccessor)
        {
            var userId = GetUserIdFromContext(httpContextAccessor);
            return await preferenceService.GetCustomerPreferencesAsync(customerId, userId);
        }

        public async Task<CustomerPreferenceDto?> GetCustomerPreference(
            int preferenceId,
            [Service] ICustomerPreferenceService preferenceService,
            [Service] IHttpContextAccessor httpContextAccessor)
        {
            var userId = GetUserIdFromContext(httpContextAccessor);
            return await preferenceService.GetPreferenceAsync(preferenceId, userId);
        }

        public async Task<CustomerPreferenceDto?> GetCustomerPreferenceByKey(
            int customerId,
            string key,
            [Service] ICustomerPreferenceService preferenceService,
            [Service] IHttpContextAccessor httpContextAccessor)
        {
            var userId = GetUserIdFromContext(httpContextAccessor);
            return await preferenceService.GetCustomerPreferenceByKeyAsync(customerId, key, userId);
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
