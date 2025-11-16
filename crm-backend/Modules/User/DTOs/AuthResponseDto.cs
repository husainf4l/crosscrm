namespace crm_backend.Modules.User.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = new UserDto();
    // Refresh token for renewing access tokens
    public string? RefreshToken { get; set; }
    // Seconds until the access token expires
    public int ExpiresIn { get; set; }
}