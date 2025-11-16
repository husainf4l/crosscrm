using FluentValidation;

namespace crm_backend.Modules.User.DTOs.Validators;

public class InviteUserDtoValidator : AbstractValidator<InviteUserDto>
{
    public InviteUserDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Please provide a valid email address")
            .MaximumLength(255)
            .WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.CompanyId)
            .GreaterThan(0)
            .WithMessage("Company ID must be greater than 0");

        RuleFor(x => x.RoleId)
            .GreaterThan(0)
            .When(x => x.RoleId.HasValue)
            .WithMessage("Role ID must be greater than 0 when specified");

        RuleFor(x => x.TeamId)
            .GreaterThan(0)
            .When(x => x.TeamId.HasValue)
            .WithMessage("Team ID must be greater than 0 when specified");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes must not exceed 500 characters");
    }
}

public class AcceptInvitationDtoValidator : AbstractValidator<AcceptInvitationDto>
{
    public AcceptInvitationDtoValidator()
    {
        RuleFor(x => x.InvitationToken)
            .NotEmpty()
            .WithMessage("Invitation token is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MinimumLength(2)
            .WithMessage("Name must be at least 2 characters long")
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters long")
            .MaximumLength(100)
            .WithMessage("Password must not exceed 100 characters");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .WithMessage("Phone number must not exceed 20 characters")
            .Matches(@"^[\d\s\+\-\(\)]+$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Phone number contains invalid characters");
    }
}
