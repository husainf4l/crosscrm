using FluentValidation;

namespace crm_backend.Modules.Opportunity.DTOs.Validators;

public class UpdateLeadSourceDtoValidator : AbstractValidator<UpdateLeadSourceDto>
{
    public UpdateLeadSourceDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Lead source name must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

