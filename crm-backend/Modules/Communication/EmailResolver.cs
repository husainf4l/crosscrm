using crm_backend.Data;
using crm_backend.GraphQL;
using crm_backend.Modules.Communication.DTOs;
using crm_backend.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace crm_backend.Modules.Communication;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class EmailResolver : BaseResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<EmailDto>> GetEmails(
        [Service] crm_backend.Modules.Communication.Services.IEmailService emailService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdOrNullAsync(httpContextAccessor, context);

        if (!companyId.HasValue)
        {
            return new List<EmailDto>();
        }

        return await emailService.GetAllEmailsAsync(companyId.Value);
    }

    [Authorize]
    public async Task<EmailDto?> GetEmail(
        int id,
        [Service] crm_backend.Modules.Communication.Services.IEmailService emailService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var email = await emailService.GetEmailByIdAsync(id);

        if (email == null || email.CompanyId != companyId)
        {
            return null;
        }

        return email;
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class EmailMutation : BaseResolver
{
    [Authorize]
    public async Task<EmailDto> CreateEmail(
        CreateEmailDto input,
        [Service] crm_backend.Modules.Communication.Services.IEmailService emailService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateEmailDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new GraphQLException($"Validation failed: {errors}");
            }

            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            var userId = GetUserId(httpContextAccessor.HttpContext);

            var modifiedInput = new CreateEmailDto
            {
                Subject = input.Subject,
                Body = input.Body,
                BodyHtml = input.BodyHtml,
                FromEmail = input.FromEmail,
                ToEmail = input.ToEmail,
                CcEmail = input.CcEmail,
                BccEmail = input.BccEmail,
                Direction = input.Direction,
                ThreadId = input.ThreadId,
                ParentEmailId = input.ParentEmailId,
                CustomerId = input.CustomerId,
                ContactId = input.ContactId,
                OpportunityId = input.OpportunityId,
                TicketId = input.TicketId,
                CompanyId = companyId,
                SentByUserId = userId
            };

            return await emailService.CreateEmailAsync(modifiedInput);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to create email");
        }
    }

    [Authorize]
    public async Task<EmailDto?> UpdateEmail(
        int id,
        UpdateEmailDto input,
        [Service] crm_backend.Modules.Communication.Services.IEmailService emailService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateEmailDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new GraphQLException($"Validation failed: {errors}");
            }

            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingEmail = await emailService.GetEmailByIdAsync(id);
            if (existingEmail == null || existingEmail.CompanyId != companyId)
            {
                throw new GraphQLException("Email not found or access denied");
            }

            return await emailService.UpdateEmailAsync(id, input);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to update email");
        }
    }

    [Authorize]
    public async Task<bool> DeleteEmail(
        int id,
        [Service] crm_backend.Modules.Communication.Services.IEmailService emailService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingEmail = await emailService.GetEmailByIdAsync(id);
            if (existingEmail == null || existingEmail.CompanyId != companyId)
            {
                throw new GraphQLException("Email not found or access denied");
            }

            return await emailService.DeleteEmailAsync(id);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to delete email");
        }
    }

    [Authorize]
    public async Task<EmailDto?> MarkEmailAsRead(
        int id,
        bool isRead,
        [Service] crm_backend.Modules.Communication.Services.IEmailService emailService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);

            var existingEmail = await emailService.GetEmailByIdAsync(id);
            if (existingEmail == null || existingEmail.CompanyId != companyId)
            {
                throw new GraphQLException("Email not found or access denied");
            }

            return await emailService.MarkEmailAsReadAsync(id, isRead);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to mark email as read");
        }
    }

    [Authorize]
    public async Task<bool> SendTestEmail(
        string toEmail,
        [Service] crm_backend.Services.IEmailService emailService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            // Test email doesn't require company validation
            var success = await emailService.SendTestEmailAsync(toEmail);
            return success;
        }
        catch (Exception ex)
        {
            throw errorHandling.HandleException(ex, "Failed to send test email");
        }
    }
}

