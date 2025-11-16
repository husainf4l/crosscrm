using crm_backend.Data;
using crm_backend.Modules.User.DTOs;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.User.Services;

public class UserService : IUserService
{
    private readonly CrmDbContext _context;

    public UserService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .Include(u => u.Company)
            .Include(u => u.UserCompanies)
                .ThenInclude(uc => uc.Company)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                Avatar = u.Avatar,
                CreatedAt = u.CreatedAt,
                CompanyId = u.CompanyId,
                CompanyName = u.Company != null ? u.Company.Name : null,
                Companies = u.UserCompanies.Select(uc => new UserCompanyDto
                {
                    CompanyId = uc.CompanyId,
                    CompanyName = uc.Company != null ? uc.Company.Name : "Unknown Company",
                    IsActive = uc.IsActive,
                    JoinedAt = uc.JoinedAt
                }).ToList()
            })
            .ToListAsync();

        return users;
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users
            .Include(u => u.Company)
            .Include(u => u.UserCompanies)
                .ThenInclude(uc => uc.Company)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return null;

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Avatar = user.Avatar,
            CreatedAt = user.CreatedAt,
            CompanyId = user.CompanyId,
            CompanyName = user.Company?.Name ?? string.Empty,
            Companies = user.UserCompanies.Select(uc => new UserCompanyDto
            {
                CompanyId = uc.CompanyId,
                CompanyName = uc.Company?.Name ?? "Unknown Company",
                IsActive = uc.IsActive,
                JoinedAt = uc.JoinedAt
            }).ToList()
        };
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
    {
        // Check if email already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"A user with email '{dto.Email}' already exists.");
        }

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Avatar = dto.Avatar
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
        }

        // Load company and user companies for response
        await _context.Entry(user).Reference(u => u.Company).LoadAsync();
        await _context.Entry(user).Collection(u => u.UserCompanies).Query().Include(uc => uc.Company).LoadAsync();

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Avatar = user.Avatar,
            CreatedAt = user.CreatedAt,
            CompanyId = user.CompanyId,
            CompanyName = user.Company?.Name ?? string.Empty,
            Companies = user.UserCompanies.Select(uc => new UserCompanyDto
            {
                CompanyId = uc.CompanyId,
                CompanyName = uc.Company != null ? uc.Company.Name! : "Unknown Company",
                IsActive = uc.IsActive,
                JoinedAt = uc.JoinedAt
            }).ToList()
        };
    }

    public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return null;

        // Check if email is being changed and if the new email already exists
        if (user.Email != dto.Email)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"A user with email '{dto.Email}' already exists.");
            }
        }

        user.Name = dto.Name;
        user.Email = dto.Email;
        user.Phone = dto.Phone;
        user.Avatar = dto.Avatar;
        user.CompanyId = dto.CompanyId;

        await _context.SaveChangesAsync();

        // Load company
        await _context.Entry(user).Reference(u => u.Company).LoadAsync();
        await _context.Entry(user).Collection(u => u.UserCompanies).Query().Include(uc => uc.Company).LoadAsync();

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Avatar = user.Avatar,
            CreatedAt = user.CreatedAt,
            CompanyId = user.CompanyId,
            CompanyName = user.Company?.Name ?? string.Empty,
            Companies = user.UserCompanies.Select(uc => new UserCompanyDto
            {
                CompanyId = uc.CompanyId,
                CompanyName = uc.Company != null ? uc.Company.Name! : "Unknown Company",
                IsActive = uc.IsActive,
                JoinedAt = uc.JoinedAt
            }).ToList()
        };
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}