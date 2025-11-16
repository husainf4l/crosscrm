namespace crm_backend.Modules.Collaboration;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Role Configuration
    public bool IsSystemRole { get; set; } = false; // System roles are predefined (Admin, Manager, etc.)

    // Multi-tenant (nullable for system roles)
    public int? CompanyId { get; set; }
    public Company.Company? Company { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

// System role names constants
public static class SystemRoles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string SalesRep = "SalesRep";
    public const string SupportAgent = "SupportAgent";
    public const string AIAgent = "AIAgent";
}

