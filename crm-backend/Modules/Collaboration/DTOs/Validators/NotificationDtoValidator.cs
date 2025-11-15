using FluentValidation;
using crm_backend.Modules.Collaboration;
using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.DTOs.Validators;

public class CreateNotificationDtoValidator : AbstractValidator<CreateNotificationDto>
{
    public CreateNotificationDtoValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(1000).WithMessage("Message must not exceed 1000 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Notification type is required")
            .Must(BeValidNotificationType).WithMessage("Invalid notification type");

        RuleFor(x => x.Priority)
            .Must(BeValidPriority).WithMessage("Invalid priority. Must be one of: Low, Medium, High, Urgent")
            .When(x => !string.IsNullOrEmpty(x.Priority));

        RuleFor(x => x.ActionUrl)
            .MaximumLength(500).WithMessage("Action URL must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ActionUrl));
    }

    private bool BeValidNotificationType(string type)
    {
        return Enum.TryParse<NotificationType>(type, true, out _);
    }

    private bool BeValidPriority(string priority)
    {
        return Enum.TryParse<NotificationPriority>(priority, true, out _);
    }
}

public class CreateNotificationPreferenceDtoValidator : AbstractValidator<CreateNotificationPreferenceDto>
{
    public CreateNotificationPreferenceDtoValidator()
    {
        RuleFor(x => x.NotificationType)
            .NotEmpty().WithMessage("Notification type is required")
            .Must(BeValidNotificationType).WithMessage("Invalid notification type");

        RuleFor(x => x.Channel)
            .NotEmpty().WithMessage("Channel is required")
            .Must(BeValidChannel).WithMessage("Invalid channel. Must be one of: InApp, Email, Push, SMS");
    }

    private bool BeValidNotificationType(string type)
    {
        return Enum.TryParse<NotificationType>(type, true, out _);
    }

    private bool BeValidChannel(string channel)
    {
        return Enum.TryParse<NotificationChannel>(channel, true, out _);
    }
}

public class UpdateNotificationPreferenceDtoValidator : AbstractValidator<UpdateNotificationPreferenceDto>
{
    public UpdateNotificationPreferenceDtoValidator()
    {
        // No required fields - all are optional
    }
}

