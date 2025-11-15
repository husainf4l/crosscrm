using FluentValidation;

namespace crm_backend.Modules.Opportunity.DTOs.Validators;

public class CreateLeadSourceDtoValidator : AbstractValidator<CreateLeadSourceDto>
{
    public CreateLeadSourceDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Lead source name is required")
            .MaximumLength(100).WithMessage("Lead source name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

