using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using crm_backend.Data;
using crm_backend.Modules.Customer.Services;
using crm_backend.Modules.User.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace crm_backend.Modules.User.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<bool> SignOutAsync(int userId);
}

public class AuthService : IAuthService
{
    private readonly CrmDbContext _context;

    public AuthService(CrmDbContext context, IConfiguration configuration, IS3Service s3Service)
    {
        _context = context;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Check if user already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (existingUser != null)
        {
            throw new Exception("User with this email already exists");
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // Generate default avatar if not provided
        var defaultAvatar = string.IsNullOrEmpty(dto.Avatar)
            ? GenerateDefaultAvatar(dto.Name, dto.Email)
            : dto.Avatar;

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Avatar = defaultAvatar,
            PasswordHash = passwordHash
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // If company is specified, create the relationship and set as active
        if (dto.CompanyId.HasValue)
        {
            var userCompany = new crm_backend.Modules.User.UserCompany
            {
                UserId = user.Id,
                CompanyId = dto.CompanyId.Value,
                IsActive = true
            };
            _context.UserCompanies.Add(userCompany);
            user.CompanyId = dto.CompanyId.Value;
            await _context.SaveChangesAsync();

            // Reload user with company information for JWT generation
            user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == user.Id) ?? user;
        }

        // Generate access token and refresh token
        var token = GenerateJwtToken(user);
        var refreshTokenString = Guid.NewGuid().ToString("N");
        var refreshToken = new RefreshToken
        {
            Token = refreshTokenString,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshTokenString,
            ExpiresIn = (int)TimeSpan.FromHours(24).TotalSeconds,
            User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Avatar = user.Avatar,
                CreatedAt = user.CreatedAt,
                CompanyId = user.CompanyId
            }
        };
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .Include(u => u.Company)
            .Include(u => u.UserCompanies)
                .ThenInclude(uc => uc.Company)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var token = GenerateJwtToken(user);
        var refreshTokenString = Guid.NewGuid().ToString("N");
        var refreshToken = new RefreshToken
        {
            Token = refreshTokenString,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshTokenString,
            ExpiresIn = (int)TimeSpan.FromHours(24).TotalSeconds,
            User = new UserDto
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
                    CompanyName = uc.Company.Name,
                    IsActive = uc.IsActive,
                    JoinedAt = uc.JoinedAt
                }).ToList()
            }
        };
    }

    public async Task<bool> SignOutAsync(int userId)
    {
        // Revoke all active refresh tokens for the user
        var tokens = _context.RefreshTokens.Where(rt => rt.UserId == userId && rt.RevokedAt == null).ToList();
        if (!tokens.Any()) return true;
        foreach (var t in tokens)
        {
            t.RevokedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();
        return true;
    }

    private string GenerateJwtToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY") ?? "default-secret-key-for-development");
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "crm-backend";
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "crm-client";

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name),
            new Claim("companyId", user.CompanyId?.ToString() ?? ""),
            new Claim("hasCompany", (user.CompanyId.HasValue && user.CompanyId.Value > 0).ToString().ToLower())
        };

        // Add company-specific claims if user has an active company
        if (user.CompanyId.HasValue && user.CompanyId.Value > 0 && user.Company != null)
        {
            claims.Add(new Claim("companyName", user.Company.Name));
            claims.Add(new Claim("activeCompanyId", user.CompanyId.Value.ToString()));
        }

        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateDefaultAvatar(string name, string email)
    {
        // Option 1: Use uploaded default avatar from S3
        var bucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME") ?? "4wk-garage-media";
        var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "me-central-1";
        var defaultAvatarUrl = $"https://{bucketName}.s3.{region}.amazonaws.com/crm-assets/default-avatar.webp";

        return defaultAvatarUrl;

        // Option 2: Fallback to Dicebear API for personalized avatars
        // var seed = email.ToLower().Trim();
        // return $"https://api.dicebear.com/7.x/initials/svg?seed={Uri.EscapeDataString(seed)}&backgroundColor=random";
    }
}
