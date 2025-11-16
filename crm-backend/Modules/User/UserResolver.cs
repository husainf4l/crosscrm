using HotChocolate;
using HotChocolate.Data;
using crm_backend.Modules.User.DTOs;
using crm_backend.Modules.User.Services;
using System.Security.Claims;
using crm_backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace crm_backend.Modules.User;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class UserResolver
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<UserDto>> GetUsers([Service] IUserService userService)
    {
        return await userService.GetAllUsersAsync();
    }

    public async Task<UserDto?> GetUser(int id, [Service] IUserService userService)
    {
        return await userService.GetUserByIdAsync(id);
    }

    [Authorize]
    public async Task<UserDto?> Me(
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

        var user = await context.Users
            .Include(u => u.Company)
            .Include(u => u.UserCompanies)
                .ThenInclude(uc => uc.Company)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new GraphQLException("User not found");
        }

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Avatar = user.Avatar,
            CreatedAt = user.CreatedAt,
            CompanyId = user.CompanyId,
            CompanyName = user.Company?.Name,
            Companies = user.UserCompanies.Select(uc => new UserCompanyDto
            {
                CompanyId = uc.CompanyId,
                CompanyName = uc.Company?.Name ?? "Unknown Company",
                IsActive = uc.IsActive,
                JoinedAt = uc.JoinedAt
            }).ToList()
        };
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class UserMutation
{
    public async Task<AuthResponseDto> Register(RegisterDto input, [Service] IAuthService authService)
    {
        try
        {
            return await authService.RegisterAsync(input);
        }
        catch (Exception ex)
        {
            throw new GraphQLException(ex.Message);
        }
    }

    public async Task<AuthResponseDto?> Login(LoginDto input, [Service] IAuthService authService)
    {
        try
        {
            return await authService.LoginAsync(input);
        }
        catch (Exception ex)
        {
            throw new GraphQLException(ex.Message);
        }
    }

    public async Task<UserDto> CreateUser(CreateUserDto input, [Service] IUserService userService)
    {
        try
        {
            return await userService.CreateUserAsync(input);
        }
        catch (InvalidOperationException ex)
        {
            throw new GraphQLException(ex.Message);
        }
        catch (Exception ex)
        {
            throw new GraphQLException($"Failed to create user: {ex.Message}");
        }
    }

    public async Task<UserDto?> UpdateUser(int id, UpdateUserDto input, [Service] IUserService userService)
    {
        try
        {
            return await userService.UpdateUserAsync(id, input);
        }
        catch (InvalidOperationException ex)
        {
            throw new GraphQLException(ex.Message);
        }
        catch (Exception ex)
        {
            throw new GraphQLException($"Failed to update user: {ex.Message}");
        }
    }

    public async Task<bool> DeleteUser(int id, [Service] IUserService userService)
    {
        return await userService.DeleteUserAsync(id);
    }

    public async Task<bool> AddUserToCompany(int userId, int companyId, [Service] CrmDbContext context)
    {
        try
        {
            var user = await context.Users.FindAsync(userId);
            var company = await context.Companies.FindAsync(companyId);

            if (user == null) throw new GraphQLException($"User with ID {userId} not found.");
            if (company == null) throw new GraphQLException($"Company with ID {companyId} not found.");

            // Check if relationship already exists
            var existing = await context.UserCompanies
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompanyId == companyId);

            if (existing != null) return true; // Already exists

            var userCompany = new crm_backend.Modules.User.UserCompany
            {
                UserId = userId,
                CompanyId = companyId,
                IsActive = false // Not active by default
            };

            context.UserCompanies.Add(userCompany);
            await context.SaveChangesAsync();
            return true;
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException($"Failed to add user to company: {ex.Message}");
        }
    }

    public async Task<bool> RemoveUserFromCompany(int userId, int companyId, [Service] CrmDbContext context)
    {
        try
        {
            var userCompany = await context.UserCompanies
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompanyId == companyId);

            if (userCompany == null) throw new GraphQLException($"User is not associated with this company.");

            // If this was the active company, clear the active company
            if (userCompany.IsActive)
            {
                var user = await context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.CompanyId = null;
                }
            }

            context.UserCompanies.Remove(userCompany);
            await context.SaveChangesAsync();
            return true;
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException($"Failed to remove user from company: {ex.Message}");
        }
    }

    public async Task<bool> SetActiveCompany(int userId, int companyId, [Service] CrmDbContext context)
    {
        try
        {
            var user = await context.Users.FindAsync(userId);
            var userCompany = await context.UserCompanies
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompanyId == companyId);

            if (user == null) throw new GraphQLException($"User with ID {userId} not found.");
            if (userCompany == null) throw new GraphQLException($"User is not associated with company ID {companyId}.");

            // Set all user's company relationships to inactive
            var userCompanies = await context.UserCompanies
                .Where(uc => uc.UserId == userId)
                .ToListAsync();

            foreach (var uc in userCompanies)
            {
                uc.IsActive = false;
            }

            // Set the specified company as active
            userCompany.IsActive = true;
            user.CompanyId = companyId;

            await context.SaveChangesAsync();
            return true;
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException($"Failed to set active company: {ex.Message}");
        }
    }

    // Temporary mutation to upload default avatar
    public async Task<string> UploadDefaultAvatar([Service] crm_backend.Modules.Customer.Services.IS3Service s3Service)
    {
        try
        {
            var avatarPath = "/Users/husain/Desktop/cross/crm-backend/6596121.webp";
            
            if (!File.Exists(avatarPath))
            {
                throw new GraphQLException("Avatar file not found");
            }

            var fileBytes = await File.ReadAllBytesAsync(avatarPath);
            var s3Key = await s3Service.UploadFileAsync("default-avatar.webp", fileBytes, "image/webp");
            
            // Generate the public URL
            var bucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME") ?? "4wk-garage-media";
            var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "me-central-1";
            var publicUrl = $"https://{bucketName}.s3.{region}.amazonaws.com/{s3Key}";
            
            return publicUrl;
        }
        catch (Exception ex)
        {
            throw new GraphQLException($"Failed to upload default avatar: {ex.Message}");
        }
    }

    [Authorize]
    public async Task<bool> SignOut(
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] IAuthService authService)
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

        try
        {
            return await authService.SignOutAsync(userId);
        }
        catch (Exception ex)
        {
            throw new GraphQLException($"Failed to sign out: {ex.Message}");
        }
    }
}