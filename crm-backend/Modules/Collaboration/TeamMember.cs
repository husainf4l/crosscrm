namespace crm_backend.Modules.Collaboration;

public class TeamMember
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public int UserId { get; set; }
    public User.User User { get; set; } = null!;

    public TeamMemberRole Role { get; set; } = TeamMemberRole.Member;
    public bool IsActive { get; set; } = true;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
}

public enum TeamMemberRole
{
    Manager,
    Member,
    Observer
}

