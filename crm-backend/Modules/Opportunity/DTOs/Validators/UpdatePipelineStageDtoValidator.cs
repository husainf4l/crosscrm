using FluentValidation;

namespace crm_backend.Modules.Opportunity.DTOs.Validators;

public class UpdatePipelineStageDtoValidator : AbstractValidator<UpdatePipelineStageDto>
{
    public UpdatePipelineStageDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Pipeline stage name must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be greater than or equal to 0")
            .When(x => x.Order.HasValue);

        RuleFor(x => x.DefaultProbability)
            .InclusiveBetween(0, 100).WithMessage("Default probability must be between 0 and 100")
            .When(x => x.DefaultProbability.HasValue);
    }
}

