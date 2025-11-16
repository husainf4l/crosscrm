namespace crm_backend.Modules.User.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Avatar { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public List<UserCompanyDto> Companies { get; set; } = new List<UserCompanyDto>();
}