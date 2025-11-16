using System.ComponentModel.DataAnnotations;

namespace crm_backend.Modules.User;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }

    // Raw token string (stored as-is for now; consider hashing in production)
    public string Token { get; set; } = string.Empty;

    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
}
