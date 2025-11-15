using crm_backend.Data;
using crm_backend.Modules.Collaboration;
using Microsoft.EntityFrameworkCore;

namespace crm_backend.Modules.Collaboration.Services;

public class PermissionSeeder
{
    private readonly CrmDbContext _context;

    public PermissionSeeder(CrmDbContext context)
    {
        _context = context;
    }

    public async Task SeedPermissionsAsync()
    {
        // Check if permissions already exist
        if (await _context.Permissions.AnyAsync())
        {
            return; // Permissions already seeded
        }

        var permissions = new List<Permission>
        {
            // Customer permissions
            new Permission { Name = Permissions.CustomerView, Description = "View customers", Module = "Customer" },
            new Permission { Name = Permissions.CustomerCreate, Description = "Create customers", Module = "Customer" },
            new Permission { Name = Permissions.CustomerEdit, Description = "Edit customers", Module = "Customer" },
            new Permission { Name = Permissions.CustomerDelete, Description = "Delete customers", Module = "Customer" },
            
            // Opportunity permissions
            new Permission { Name = Permissions.OpportunityView, Description = "View opportunities", Module = "Opportunity" },
            new Permission { Name = Permissions.OpportunityCreate, Description = "Create opportunities", Module = "Opportunity" },
            new Permission { Name = Permissions.OpportunityEdit, Description = "Edit opportunities", Module = "Opportunity" },
            new Permission { Name = Permissions.OpportunityDelete, Description = "Delete opportunities", Module = "Opportunity" },
            
            // Team permissions
            new Permission { Name = Permissions.TeamView, Description = "View teams", Module = "Team" },
            new Permission { Name = Permissions.TeamCreate, Description = "Create teams", Module = "Team" },
            new Permission { Name = Permissions.TeamEdit, Description = "Edit teams", Module = "Team" },
            new Permission { Name = Permissions.TeamDelete, Description = "Delete teams", Module = "Team" },
            new Permission { Name = Permissions.TeamManageMembers, Description = "Manage team members", Module = "Team" },
            
            // Admin permissions
            new Permission { Name = Permissions.AdminAccess, Description = "Admin access", Module = "Admin" },
            new Permission { Name = Permissions.UserManagement, Description = "Manage users", Module = "Admin" },
            new Permission { Name = Permissions.CompanySettings, Description = "Manage company settings", Module = "Admin" }
        };

        _context.Permissions.AddRange(permissions);
        await _context.SaveChangesAsync();
    }
}

