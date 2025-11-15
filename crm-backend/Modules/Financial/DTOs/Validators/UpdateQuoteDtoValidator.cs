using FluentValidation;

namespace crm_backend.Modules.Financial.DTOs.Validators;

public class UpdateQuoteDtoValidator : AbstractValidator<UpdateQuoteDto>
{
    public UpdateQuoteDtoValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.TaxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Tax amount must be greater than or equal to 0")
            .When(x => x.TaxAmount.HasValue);

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount amount must be greater than or equal to 0")
            .When(x => x.DiscountAmount.HasValue);

        RuleFor(x => x.Currency)
            .Length(3).WithMessage("Currency must be a 3-character code (e.g., USD)")
            .When(x => !string.IsNullOrWhiteSpace(x.Currency));

        RuleFor(x => x.ValidUntil)
            .GreaterThan(DateTime.UtcNow).WithMessage("Valid until date must be in the future")
            .When(x => x.ValidUntil.HasValue);
    }
}

