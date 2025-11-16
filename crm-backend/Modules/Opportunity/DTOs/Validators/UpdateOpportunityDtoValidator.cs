using FluentValidation;

namespace crm_backend.Modules.Opportunity.DTOs.Validators;

public class UpdateOpportunityDtoValidator : AbstractValidator<UpdateOpportunityDto>
{
    public UpdateOpportunityDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Opportunity name must not exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Amount must be greater than or equal to 0")
            .When(x => x.Amount.HasValue);

        RuleFor(x => x.Currency)
            .Length(3).WithMessage("Currency must be a 3-character code (e.g., USD)")
            .When(x => !string.IsNullOrWhiteSpace(x.Currency));

        RuleFor(x => x.Probability)
            .InclusiveBetween(0, 100).WithMessage("Probability must be between 0 and 100")
            .When(x => x.Probability.HasValue);

        RuleFor(x => x.ExpectedCloseDate)
            .GreaterThan(DateTime.UtcNow.Date).WithMessage("Expected close date must be in the future")
            .When(x => x.ExpectedCloseDate.HasValue);
    }
}

