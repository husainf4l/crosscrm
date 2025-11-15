using FluentValidation;

namespace crm_backend.Modules.Opportunity.DTOs.Validators;

public class CreateOpportunityDtoValidator : AbstractValidator<CreateOpportunityDto>
{
    public CreateOpportunityDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Opportunity name is required")
            .MaximumLength(200).WithMessage("Opportunity name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Amount must be greater than or equal to 0");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-character code (e.g., USD)");

        RuleFor(x => x.Probability)
            .InclusiveBetween(0, 100).WithMessage("Probability must be between 0 and 100");

        RuleFor(x => x.PipelineStageId)
            .GreaterThan(0).WithMessage("Pipeline stage ID is required");

        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer ID is required");

        RuleFor(x => x.ExpectedCloseDate)
            .GreaterThan(DateTime.UtcNow.Date).WithMessage("Expected close date must be in the future")
            .When(x => x.ExpectedCloseDate.HasValue);
    }
}

