namespace crm_backend.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null);
    Task<bool> SendTestEmailAsync(string toEmail);
    Task<bool> SendInvitationEmailAsync(string toEmail, string invitationToken, string inviterName, string companyName);
    Task<bool> SendWelcomeEmailAsync(string toEmail, string userName, string companyName);
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetToken, string userName);
    Task<bool> SendNotificationEmailAsync(string toEmail, string subject, string message, string? actionUrl = null);
}
