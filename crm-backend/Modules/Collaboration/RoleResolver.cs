using crm_backend.Data;
using crm_backend.GraphQL;
using crm_backend.Modules.Collaboration.DTOs;
using crm_backend.Modules.Collaboration.Services;
using crm_backend.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace crm_backend.Modules.Collaboration;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class RoleResolver : BaseResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<RoleDto>> GetRoles(
        [Service] IRoleService roleService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdOrNullAsync(httpContextAccessor, context);

        return await roleService.GetAllRolesAsync(companyId);
    }

    [Authorize]
    public async Task<RoleDto?> GetRole(
        int id,
        [Service] IRoleService roleService)
    {
        return await roleService.GetRoleByIdAsync(id);
    }

    [Authorize]
    public async Task<IEnumerable<PermissionDto>> GetPermissions(
        [Service] IRoleService roleService)
    {
        return await roleService.GetAllPermissionsAsync();
    }

    [Authorize]
    public async Task<IEnumerable<PermissionDto>> GetRolePermissions(
        int roleId,
        [Service] IRoleService roleService)
    {
        return await roleService.GetRolePermissionsAsync(roleId);
    }

    [Authorize]
    public async Task<IEnumerable<UserRoleDto>> GetUserRoles(
        int userId,
        [Service] IUserRoleService userRoleService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdOrNullAsync(httpContextAccessor, context);
        return await userRoleService.GetUserRolesAsync(userId, companyId);
    }

    [Authorize]
    public async Task<IEnumerable<string>> GetMyPermissions(
        [Service] IUserRoleService userRoleService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var userId = GetUserId(httpContextAccessor.HttpContext);
        return await userRoleService.GetUserPermissionsAsync(userId, companyId);
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class RoleMutation : BaseResolver
{
    [Authorize]
    public async Task<RoleDto> CreateRole(
        CreateRoleDto input,
        [Service] IRoleService roleService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateRoleDto> validator,
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

            var modifiedInput = new CreateRoleDto
            {
                Name = input.Name,
                Description = input.Description,
                CompanyId = companyId,
                PermissionIds = input.PermissionIds
            };

            return await roleService.CreateRoleAsync(modifiedInput);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to create role");
        }
    }

    [Authorize]
    public async Task<RoleDto?> UpdateRole(
        int id,
        UpdateRoleDto input,
        [Service] IRoleService roleService,
        [Service] IValidator<UpdateRoleDto> validator,
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

            return await roleService.UpdateRoleAsync(id, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to update role");
        }
    }

    [Authorize]
    public async Task<bool> DeleteRole(
        int id,
        [Service] IRoleService roleService,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            return await roleService.DeleteRoleAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to delete role");
        }
    }

    [Authorize]
    public async Task<bool> AssignPermissionToRole(
        int roleId,
        int permissionId,
        [Service] IRoleService roleService,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            return await roleService.AssignPermissionToRoleAsync(roleId, permissionId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to assign permission to role");
        }
    }

    [Authorize]
    public async Task<bool> RemovePermissionFromRole(
        int roleId,
        int permissionId,
        [Service] IRoleService roleService,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            return await roleService.RemovePermissionFromRoleAsync(roleId, permissionId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to remove permission from role");
        }
    }

    [Authorize]
    public async Task<UserRoleDto> AssignUserRole(
        AssignUserRoleDto input,
        [Service] IUserRoleService userRoleService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var assignedByUserId = GetUserId(httpContextAccessor.HttpContext);
            return await userRoleService.AssignUserRoleAsync(input, assignedByUserId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to assign role to user");
        }
    }

    [Authorize]
    public async Task<bool> RemoveUserRole(
        int userRoleId,
        [Service] IUserRoleService userRoleService,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            return await userRoleService.RemoveUserRoleAsync(userRoleId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to remove role from user");
        }
    }
}

