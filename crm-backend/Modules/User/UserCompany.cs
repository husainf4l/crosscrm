namespace crm_backend.Modules.User;

public class UserCompany
{
    public int UserId { get; set; }
    public crm_backend.Modules.User.User User { get; set; } = null!;

    public int CompanyId { get; set; }
    public crm_backend.Modules.Company.Company Company { get; set; } = null!;

    public bool IsActive { get; set; } = false; // Track which is the active company
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
