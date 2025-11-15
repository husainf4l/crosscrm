namespace crm_backend.Modules.Collaboration;

public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty; // e.g., "Customer", "Opportunity", "Invoice"
    
    // Navigation
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

// Permission names constants
public static class Permissions
{
    // Customer permissions
    public const string CustomerView = "Customer.View";
    public const string CustomerCreate = "Customer.Create";
    public const string CustomerEdit = "Customer.Edit";
    public const string CustomerDelete = "Customer.Delete";
    
    // Opportunity permissions
    public const string OpportunityView = "Opportunity.View";
    public const string OpportunityCreate = "Opportunity.Create";
    public const string OpportunityEdit = "Opportunity.Edit";
    public const string OpportunityDelete = "Opportunity.Delete";
    
    // Team permissions
    public const string TeamView = "Team.View";
    public const string TeamCreate = "Team.Create";
    public const string TeamEdit = "Team.Edit";
    public const string TeamDelete = "Team.Delete";
    public const string TeamManageMembers = "Team.ManageMembers";
    
    // Admin permissions
    public const string AdminAccess = "Admin.Access";
    public const string UserManagement = "User.Manage";
    public const string CompanySettings = "Company.Settings";
}

