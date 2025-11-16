using SendGrid;
using SendGrid.Helpers.Mail;

namespace crm_backend.Services;

public class EmailService : IEmailService
{
    private readonly ISendGridClient _sendGridClient;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _frontendUrl;
    private readonly ILogger<EmailService> _logger;

    public EmailService(ISendGridClient sendGridClient, IConfiguration configuration, ILogger<EmailService> logger)
    {
        _sendGridClient = sendGridClient;
        _fromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL") ?? throw new ArgumentException("FROM_EMAIL not configured");
        _fromName = Environment.GetEnvironmentVariable("FROM_NAME") ?? "Cross CRM";
        _frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:3000";
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null)
    {
        try
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }

            var responseBody = await response.Body.ReadAsStringAsync();
            _logger.LogWarning("Failed to send email to {Email}. Status: {Status}, Response: {Response}",
                toEmail, response.StatusCode, responseBody);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", toEmail);
            return false;
        }
    }

    public async Task<bool> SendTestEmailAsync(string toEmail)
    {
        var subject = "Cross CRM - Test Email";
        var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Test Email</title>
</head>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='text-align: center; margin-bottom: 30px;'>
        <h1 style='color: #2563eb;'>Cross CRM</h1>
        <h2 style='color: #16a34a;'>✅ Email Test Successful!</h2>
    </div>
    
    <div style='background: #f0fdf4; padding: 30px; border-radius: 8px; border: 1px solid #bbf7d0;'>
        <p style='font-size: 16px; margin-bottom: 20px;'>Hello!</p>
        <p style='font-size: 16px; margin-bottom: 20px;'>
            This is a test email from your Cross CRM system to verify that email delivery is working correctly.
        </p>
        <p style='font-size: 14px; color: #16a34a;'>
            <strong>✅ SendGrid Configuration: Working</strong><br>
            <strong>✅ Domain: aqlaan.com</strong><br>
            <strong>✅ From Address: {_fromEmail}</strong>
        </p>
    </div>
    
    <div style='text-align: center; color: #6b7280; font-size: 12px; margin-top: 30px;'>
        <p>Test sent at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
        <p>The Cross CRM Team</p>
    </div>
</body>
</html>";

        var plainTextContent = $@"
Cross CRM - Test Email

✅ Email Test Successful!

This is a test email from your Cross CRM system to verify that email delivery is working correctly.

Configuration Details:
✅ SendGrid Configuration: Working
✅ Domain: aqlaan.com  
✅ From Address: {_fromEmail}

Test sent at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC

The Cross CRM Team
        ";

        return await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
    }

    public async Task<bool> SendInvitationEmailAsync(string toEmail, string invitationToken, string inviterName, string companyName)
    {
        var subject = $"You're invited to join {companyName} on Cross CRM";
        var invitationLink = $"{_frontendUrl}/accept-invitation/{invitationToken}";

        var htmlContent = GetInvitationEmailTemplate(inviterName, companyName, invitationLink);
        var plainTextContent = $@"
Hi there,

{inviterName} has invited you to join {companyName} on Cross CRM.

To accept this invitation and create your account, please visit:
{invitationLink}

This invitation will expire in 7 days.

If you have any questions, please contact {inviterName} directly.

Best regards,
The Cross CRM Team
        ";

        return await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
    }

    public async Task<bool> SendWelcomeEmailAsync(string toEmail, string userName, string companyName)
    {
        var subject = $"Welcome to {companyName} on Cross CRM!";
        var loginUrl = $"{_frontendUrl}/login";

        var htmlContent = GetWelcomeEmailTemplate(userName, companyName, loginUrl);
        var plainTextContent = $@"
Hi {userName},

Welcome to {companyName} on Cross CRM!

Your account has been successfully created. You can now access the CRM system at:
{loginUrl}

If you need any assistance getting started, please don't hesitate to reach out to your team administrator.

Best regards,
The Cross CRM Team
        ";

        return await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
    }

    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetToken, string userName)
    {
        var subject = "Reset Your Cross CRM Password";
        var resetLink = $"{_frontendUrl}/reset-password/{resetToken}";

        var htmlContent = GetPasswordResetEmailTemplate(userName, resetLink);
        var plainTextContent = $@"
Hi {userName},

You requested to reset your password for Cross CRM.

To reset your password, please click the link below:
{resetLink}

This link will expire in 1 hour for security reasons.

If you didn't request this password reset, please ignore this email.

Best regards,
The Cross CRM Team
        ";

        return await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
    }

    public async Task<bool> SendNotificationEmailAsync(string toEmail, string subject, string message, string? actionUrl = null)
    {
        var htmlContent = GetNotificationEmailTemplate(subject, message, actionUrl);
        var plainTextContent = $@"
{message}

{(actionUrl != null ? $"View details: {actionUrl}" : "")}

Best regards,
The Cross CRM Team
        ";

        return await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
    }

    private string GetInvitationEmailTemplate(string inviterName, string companyName, string invitationLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>You're invited to join {companyName}</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='text-align: center; margin-bottom: 30px;'>
        <h1 style='color: #2563eb; margin-bottom: 10px;'>Cross CRM</h1>
        <h2 style='color: #333; margin-top: 0;'>You're Invited!</h2>
    </div>
    
    <div style='background: #f8fafc; padding: 30px; border-radius: 8px; margin-bottom: 30px;'>
        <p style='font-size: 16px; margin-bottom: 20px;'>Hi there,</p>
        
        <p style='font-size: 16px; margin-bottom: 20px;'>
            <strong>{inviterName}</strong> has invited you to join <strong>{companyName}</strong> on Cross CRM.
        </p>
        
        <p style='font-size: 16px; margin-bottom: 30px;'>
            Cross CRM is a powerful customer relationship management platform that will help you stay organized and grow your business.
        </p>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{invitationLink}' style='background: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>
                Accept Invitation
            </a>
        </div>
        
        <p style='font-size: 14px; color: #6b7280; margin-top: 30px;'>
            This invitation will expire in 7 days. If you have any questions, please contact {inviterName} directly.
        </p>
    </div>
    
    <div style='text-align: center; color: #6b7280; font-size: 12px;'>
        <p>Best regards,<br>The Cross CRM Team</p>
        <p style='margin-top: 20px;'>
            If you're having trouble clicking the button, copy and paste this URL into your browser:<br>
            <a href='{invitationLink}' style='color: #2563eb;'>{invitationLink}</a>
        </p>
    </div>
</body>
</html>";
    }

    private string GetWelcomeEmailTemplate(string userName, string companyName, string loginUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Welcome to {companyName}!</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='text-align: center; margin-bottom: 30px;'>
        <h1 style='color: #2563eb; margin-bottom: 10px;'>Cross CRM</h1>
        <h2 style='color: #16a34a; margin-top: 0;'>Welcome to the team!</h2>
    </div>
    
    <div style='background: #f0fdf4; padding: 30px; border-radius: 8px; margin-bottom: 30px; border: 1px solid #bbf7d0;'>
        <p style='font-size: 16px; margin-bottom: 20px;'>Hi {userName},</p>
        
        <p style='font-size: 16px; margin-bottom: 20px;'>
            Welcome to <strong>{companyName}</strong> on Cross CRM! Your account has been successfully created.
        </p>
        
        <p style='font-size: 16px; margin-bottom: 30px;'>
            You can now access the CRM system and start collaborating with your team.
        </p>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{loginUrl}' style='background: #16a34a; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>
                Access CRM System
            </a>
        </div>
        
        <p style='font-size: 14px; color: #15803d; margin-top: 30px;'>
            If you need any assistance getting started, please don't hesitate to reach out to your team administrator.
        </p>
    </div>
    
    <div style='text-align: center; color: #6b7280; font-size: 12px;'>
        <p>Best regards,<br>The Cross CRM Team</p>
    </div>
</body>
</html>";
    }

    private string GetPasswordResetEmailTemplate(string userName, string resetLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reset Your Password</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='text-align: center; margin-bottom: 30px;'>
        <h1 style='color: #2563eb; margin-bottom: 10px;'>Cross CRM</h1>
        <h2 style='color: #dc2626; margin-top: 0;'>Password Reset Request</h2>
    </div>
    
    <div style='background: #fef2f2; padding: 30px; border-radius: 8px; margin-bottom: 30px; border: 1px solid #fecaca;'>
        <p style='font-size: 16px; margin-bottom: 20px;'>Hi {userName},</p>
        
        <p style='font-size: 16px; margin-bottom: 20px;'>
            You requested to reset your password for Cross CRM.
        </p>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{resetLink}' style='background: #dc2626; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>
                Reset Password
            </a>
        </div>
        
        <p style='font-size: 14px; color: #991b1b; margin-top: 30px;'>
            This link will expire in 1 hour for security reasons.
        </p>
        
        <p style='font-size: 14px; color: #991b1b;'>
            If you didn't request this password reset, please ignore this email.
        </p>
    </div>
    
    <div style='text-align: center; color: #6b7280; font-size: 12px;'>
        <p>Best regards,<br>The Cross CRM Team</p>
    </div>
</body>
</html>";
    }

    private string GetNotificationEmailTemplate(string title, string message, string? actionUrl)
    {
        var actionButton = actionUrl != null ?
            $@"<div style='text-align: center; margin: 30px 0;'>
                <a href='{actionUrl}' style='background: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>
                    View Details
                </a>
            </div>" : "";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{title}</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='text-align: center; margin-bottom: 30px;'>
        <h1 style='color: #2563eb; margin-bottom: 10px;'>Cross CRM</h1>
        <h2 style='color: #333; margin-top: 0;'>{title}</h2>
    </div>
    
    <div style='background: #f8fafc; padding: 30px; border-radius: 8px; margin-bottom: 30px;'>
        <p style='font-size: 16px; white-space: pre-line;'>{message}</p>
        
        {actionButton}
    </div>
    
    <div style='text-align: center; color: #6b7280; font-size: 12px;'>
        <p>Best regards,<br>The Cross CRM Team</p>
    </div>
</body>
</html>";
    }
}
