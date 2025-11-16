using FluentValidation;

namespace crm_backend.Modules.Communication.DTOs.Validators;

public class CreateEmailDtoValidator : AbstractValidator<CreateEmailDto>
{
    public CreateEmailDtoValidator()
    {
        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Email subject is required")
            .MaximumLength(500).WithMessage("Subject must not exceed 500 characters");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Email body is required")
            .MaximumLength(10000).WithMessage("Body must not exceed 10000 characters");

        RuleFor(x => x.FromEmail)
            .NotEmpty().WithMessage("From email is required")
            .EmailAddress().WithMessage("Invalid from email address");

        RuleFor(x => x.ToEmail)
            .NotEmpty().WithMessage("To email is required")
            .EmailAddress().WithMessage("Invalid to email address");

        RuleFor(x => x.CcEmail)
            .EmailAddress().WithMessage("Invalid CC email address")
            .When(x => !string.IsNullOrWhiteSpace(x.CcEmail));

        RuleFor(x => x.BccEmail)
            .EmailAddress().WithMessage("Invalid BCC email address")
            .When(x => !string.IsNullOrWhiteSpace(x.BccEmail));
    }
}

public class UpdateEmailDtoValidator : AbstractValidator<UpdateEmailDto>
{
    public UpdateEmailDtoValidator()
    {
        RuleFor(x => x.Subject)
            .MaximumLength(500).WithMessage("Subject must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Subject));

        RuleFor(x => x.Body)
            .MaximumLength(10000).WithMessage("Body must not exceed 10000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Body));
    }
}

