using System.Security.Cryptography;
using crm_backend.Data;
using crm_backend.Modules.Collaboration;
using crm_backend.Modules.User.DTOs;
using crm_backend.Services;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.User.Services;

public class UserInvitationService : IUserInvitationService
{
    private readonly CrmDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IAuthService _authService;
    private readonly ILogger<UserInvitationService> _logger;

    public UserInvitationService(
        CrmDbContext context,
        IEmailService emailService,
        IAuthService authService,
        ILogger<UserInvitationService> logger)
    {
        _context = context;
        _emailService = emailService;
        _authService = authService;
        _logger = logger;
    }

    public async Task<UserInvitationDto> InviteUserAsync(InviteUserDto inviteDto, int invitedByUserId)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == inviteDto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            // Check for pending invitation
            var existingInvitation = await _context.UserInvitations
                .FirstOrDefaultAsync(ui => ui.Email == inviteDto.Email &&
                                         ui.CompanyId == inviteDto.CompanyId &&
                                         ui.Status == InvitationStatus.Pending);

            if (existingInvitation != null)
            {
                throw new InvalidOperationException("A pending invitation already exists for this email");
            }

            // Get inviting user and company details
            var invitingUser = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == invitedByUserId);

            if (invitingUser == null)
            {
                throw new InvalidOperationException("Inviting user not found");
            }

            var company = await _context.Companies.FindAsync(inviteDto.CompanyId);
            if (company == null)
            {
                throw new InvalidOperationException("Company not found");
            }

            // Validate role exists
            if (inviteDto.RoleId.HasValue)
            {
                var roleExists = await _context.Roles.AnyAsync(r => r.Id == inviteDto.RoleId.Value);
                if (!roleExists)
                {
                    throw new InvalidOperationException($"Role with ID {inviteDto.RoleId.Value} not found");
                }
            }
            else
            {
                // If no role specified, assign default SalesRep role
                await EnsureSystemRolesExistAsync();
                var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == SystemRoles.SalesRep && r.IsSystemRole);
                if (defaultRole != null)
                {
                    inviteDto.RoleId = defaultRole.Id;
                }
            }

            // Validate team exists (if specified)
            if (inviteDto.TeamId.HasValue)
            {
                var teamExists = await _context.Teams.AnyAsync(t => t.Id == inviteDto.TeamId.Value);
                if (!teamExists)
                {
                    throw new InvalidOperationException($"Team with ID {inviteDto.TeamId.Value} not found");
                }
            }

            // Create invitation
            var invitation = new UserInvitation
            {
                Email = inviteDto.Email,
                InvitationToken = GenerateInvitationToken(),
                CompanyId = inviteDto.CompanyId,
                InvitedByUserId = invitedByUserId,
                RoleId = inviteDto.RoleId,
                TeamId = inviteDto.TeamId,
                Status = InvitationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days expiration
                Notes = inviteDto.Notes
            };

            _context.UserInvitations.Add(invitation);
            await _context.SaveChangesAsync();

            // Send invitation email
            var emailSent = await _emailService.SendInvitationEmailAsync(
                inviteDto.Email,
                invitation.InvitationToken,
                invitingUser.Name,
                company.Name
            );

            if (!emailSent)
            {
                _logger.LogWarning("Failed to send invitation email to {Email}", inviteDto.Email);
            }

            // Return DTO
            return await GetInvitationDtoAsync(invitation);
        }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
        {
            var errorMessage = pgEx.SqlState switch
            {
                "23503" => pgEx.ConstraintName switch
                {
                    "FK_UserInvitations_Roles_RoleId" => "The specified role does not exist",
                    "FK_UserInvitations_Teams_TeamId" => "The specified team does not exist",
                    "FK_UserInvitations_Companies_CompanyId" => "The specified company does not exist",
                    "FK_UserInvitations_Users_InvitedByUserId" => "The inviting user does not exist",
                    _ => "A referenced entity does not exist"
                },
                "23505" => "An invitation with these details already exists",
                _ => "A database constraint was violated"
            };

            _logger.LogError(ex, "Database constraint violation when inviting user {Email} to company {CompanyId}: {Error}",
                inviteDto.Email, inviteDto.CompanyId, errorMessage);
            throw new InvalidOperationException($"Failed to invite user: {errorMessage}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inviting user {Email} to company {CompanyId}", inviteDto.Email, inviteDto.CompanyId);
            throw new InvalidOperationException($"Failed to invite user: {ex.Message}");
        }
    }

    public async Task<InvitationResponseDto> AcceptInvitationAsync(AcceptInvitationDto acceptDto)
    {
        try
        {
            var invitation = await _context.UserInvitations
                .Include(ui => ui.Company)
                .Include(ui => ui.InvitedByUser)
                .Include(ui => ui.Role)
                .Include(ui => ui.Team)
                .FirstOrDefaultAsync(ui => ui.InvitationToken == acceptDto.InvitationToken);

            if (invitation == null)
            {
                return new InvitationResponseDto
                {
                    Success = false,
                    Message = "Invalid invitation token"
                };
            }

            if (invitation.Status != InvitationStatus.Pending)
            {
                return new InvitationResponseDto
                {
                    Success = false,
                    Message = "This invitation has already been processed"
                };
            }

            if (invitation.ExpiresAt < DateTime.UtcNow)
            {
                invitation.Status = InvitationStatus.Expired;
                await _context.SaveChangesAsync();

                return new InvitationResponseDto
                {
                    Success = false,
                    Message = "This invitation has expired"
                };
            }

            // Check if user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == invitation.Email);
            if (existingUser != null)
            {
                return new InvitationResponseDto
                {
                    Success = false,
                    Message = "A user with this email already exists"
                };
            }

            // Register the new user
            var registerDto = new RegisterDto
            {
                Name = acceptDto.Name,
                Email = invitation.Email,
                Password = acceptDto.Password,
                Phone = acceptDto.Phone
            };

            var authResponse = await _authService.RegisterAsync(registerDto);
            if (authResponse == null)
            {
                return new InvitationResponseDto
                {
                    Success = false,
                    Message = "Failed to create user account"
                };
            }

            // Add user to company
            var newUser = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Email == invitation.Email);
            if (newUser != null)
            {
                // Add to UserCompanies
                var userCompany = new UserCompany
                {
                    UserId = newUser.Id,
                    CompanyId = invitation.CompanyId,
                    IsActive = true,
                    JoinedAt = DateTime.UtcNow
                };
                _context.UserCompanies.Add(userCompany);

                // Set active company
                newUser.CompanyId = invitation.CompanyId;

                // Assign role if specified
                if (invitation.RoleId.HasValue)
                {
                    var userRole = new crm_backend.Modules.Collaboration.UserRole
                    {
                        UserId = newUser.Id,
                        RoleId = invitation.RoleId.Value,
                        CompanyId = invitation.CompanyId,
                        AssignedByUserId = invitation.InvitedByUserId,
                        AssignedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.UserRoles.Add(userRole);
                }

                // Add to team if specified
                if (invitation.TeamId.HasValue)
                {
                    var teamMember = new crm_backend.Modules.Collaboration.TeamMember
                    {
                        TeamId = invitation.TeamId.Value,
                        UserId = newUser.Id,
                        Role = crm_backend.Modules.Collaboration.TeamMemberRole.Member,
                        JoinedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.TeamMembers.Add(teamMember);
                }

                // Update invitation status
                invitation.Status = InvitationStatus.Accepted;
                invitation.AcceptedAt = DateTime.UtcNow;
                invitation.AcceptedByUserId = newUser.Id;

                await _context.SaveChangesAsync();

                // Reload user with company to generate updated JWT token
                newUser = await _context.Users
                    .Include(u => u.Company)
                    .FirstOrDefaultAsync(u => u.Id == newUser.Id);

                // Generate new auth response with updated company information
                var updatedAuthResponse = await _authService.LoginAsync(new LoginDto
                {
                    Email = invitation.Email,
                    Password = acceptDto.Password
                });

                return new InvitationResponseDto
                {
                    Success = true,
                    Message = "Invitation accepted successfully",
                    Invitation = await GetInvitationDtoAsync(invitation),
                    AuthResponse = updatedAuthResponse
                };
            }

            return new InvitationResponseDto
            {
                Success = false,
                Message = "Failed to complete invitation process"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting invitation with token {Token}", acceptDto.InvitationToken);
            throw;
        }
    }

    public async Task<UserInvitationDto?> GetInvitationByTokenAsync(string token)
    {
        var invitation = await _context.UserInvitations
            .Include(ui => ui.Company)
            .Include(ui => ui.InvitedByUser)
            .Include(ui => ui.AcceptedByUser)
            .Include(ui => ui.Role)
            .Include(ui => ui.Team)
            .FirstOrDefaultAsync(ui => ui.InvitationToken == token);

        return invitation != null ? await GetInvitationDtoAsync(invitation) : null;
    }

    public async Task<IEnumerable<UserInvitationDto>> GetCompanyInvitationsAsync(int companyId)
    {
        var invitations = await _context.UserInvitations
            .Include(ui => ui.Company)
            .Include(ui => ui.InvitedByUser)
            .Include(ui => ui.AcceptedByUser)
            .Include(ui => ui.Role)
            .Include(ui => ui.Team)
            .Where(ui => ui.CompanyId == companyId)
            .OrderByDescending(ui => ui.CreatedAt)
            .ToListAsync();

        var result = new List<UserInvitationDto>();
        foreach (var invitation in invitations)
        {
            result.Add(await GetInvitationDtoAsync(invitation));
        }
        return result;
    }

    public async Task<IEnumerable<UserInvitationDto>> GetUserInvitationsAsync(int userId)
    {
        var invitations = await _context.UserInvitations
            .Include(ui => ui.Company)
            .Include(ui => ui.InvitedByUser)
            .Include(ui => ui.AcceptedByUser)
            .Include(ui => ui.Role)
            .Include(ui => ui.Team)
            .Where(ui => ui.InvitedByUserId == userId)
            .OrderByDescending(ui => ui.CreatedAt)
            .ToListAsync();

        var result = new List<UserInvitationDto>();
        foreach (var invitation in invitations)
        {
            result.Add(await GetInvitationDtoAsync(invitation));
        }
        return result;
    }

    public async Task<bool> CancelInvitationAsync(int invitationId, int userId)
    {
        var invitation = await _context.UserInvitations
            .FirstOrDefaultAsync(ui => ui.Id == invitationId &&
                                     (ui.InvitedByUserId == userId ||
                                      _context.Users.Any(u => u.Id == userId && u.CompanyId == ui.CompanyId)));

        if (invitation == null || invitation.Status != InvitationStatus.Pending)
        {
            return false;
        }

        invitation.Status = InvitationStatus.Cancelled;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResendInvitationAsync(int invitationId, int userId)
    {
        var invitation = await _context.UserInvitations
            .Include(ui => ui.Company)
            .Include(ui => ui.InvitedByUser)
            .FirstOrDefaultAsync(ui => ui.Id == invitationId &&
                                     ui.InvitedByUserId == userId &&
                                     ui.Status == InvitationStatus.Pending);

        if (invitation == null || invitation.ExpiresAt < DateTime.UtcNow)
        {
            return false;
        }

        // Extend expiration and send email again
        invitation.ExpiresAt = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        var emailSent = await _emailService.SendInvitationEmailAsync(
            invitation.Email,
            invitation.InvitationToken,
            invitation.InvitedByUser.Name,
            invitation.Company.Name
        );

        return emailSent;
    }

    public async Task CleanupExpiredInvitationsAsync()
    {
        var expiredInvitations = await _context.UserInvitations
            .Where(ui => ui.Status == InvitationStatus.Pending && ui.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        foreach (var invitation in expiredInvitations)
        {
            invitation.Status = InvitationStatus.Expired;
        }

        if (expiredInvitations.Any())
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Marked {Count} invitations as expired", expiredInvitations.Count);
        }
    }

    private static string GenerateInvitationToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var tokenData = new byte[32];
        rng.GetBytes(tokenData);
        return Convert.ToBase64String(tokenData).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private async Task<UserInvitationDto> GetInvitationDtoAsync(UserInvitation invitation)
    {
        // Ensure related entities are loaded
        await _context.Entry(invitation)
            .Reference(ui => ui.Company)
            .LoadAsync();
        await _context.Entry(invitation)
            .Reference(ui => ui.InvitedByUser)
            .LoadAsync();
        await _context.Entry(invitation)
            .Reference(ui => ui.AcceptedByUser)
            .LoadAsync();
        await _context.Entry(invitation)
            .Reference(ui => ui.Role)
            .LoadAsync();
        await _context.Entry(invitation)
            .Reference(ui => ui.Team)
            .LoadAsync();

        return new UserInvitationDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            InvitationToken = invitation.InvitationToken,
            CompanyId = invitation.CompanyId,
            CompanyName = invitation.Company?.Name ?? "Unknown Company",
            InvitedByUserId = invitation.InvitedByUserId,
            InvitedByUserName = invitation.InvitedByUser?.Name ?? "Unknown User",
            RoleId = invitation.RoleId,
            RoleName = invitation.Role?.Name,
            TeamId = invitation.TeamId,
            TeamName = invitation.Team?.Name,
            Status = invitation.Status,
            CreatedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt,
            AcceptedAt = invitation.AcceptedAt,
            AcceptedByUserId = invitation.AcceptedByUserId,
            AcceptedByUserName = invitation.AcceptedByUser?.Name,
            Notes = invitation.Notes
        };
    }

    private async Task EnsureSystemRolesExistAsync()
    {
        var systemRoles = new[]
        {
            new { Name = SystemRoles.Admin, Description = "System Administrator with full access" },
            new { Name = SystemRoles.Manager, Description = "Manager with team oversight capabilities" },
            new { Name = SystemRoles.SalesRep, Description = "Sales Representative" },
            new { Name = SystemRoles.SupportAgent, Description = "Customer Support Agent" },
            new { Name = SystemRoles.AIAgent, Description = "AI Agent for automated processes" }
        };

        foreach (var roleInfo in systemRoles)
        {
            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleInfo.Name && r.IsSystemRole);
            if (existingRole == null)
            {
                var newRole = new Role
                {
                    Name = roleInfo.Name,
                    Description = roleInfo.Description,
                    IsSystemRole = true,
                    CompanyId = null // System roles are not company-specific
                };

                _context.Roles.Add(newRole);
            }
        }

        await _context.SaveChangesAsync();
    }
}
