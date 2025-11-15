using FluentValidation;

namespace crm_backend.Modules.Opportunity.DTOs.Validators;

public class CreatePipelineStageDtoValidator : AbstractValidator<CreatePipelineStageDto>
{
    public CreatePipelineStageDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Pipeline stage name is required")
            .MaximumLength(100).WithMessage("Pipeline stage name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be greater than or equal to 0");

        RuleFor(x => x.DefaultProbability)
            .InclusiveBetween(0, 100).WithMessage("Default probability must be between 0 and 100");
    }
}

