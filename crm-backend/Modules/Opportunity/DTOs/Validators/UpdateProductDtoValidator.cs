using FluentValidation;

namespace crm_backend.Modules.Opportunity.DTOs.Validators;

public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.SKU)
            .MaximumLength(100).WithMessage("SKU must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.SKU));

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.Currency)
            .Length(3).WithMessage("Currency must be a 3-character code (e.g., USD)")
            .When(x => !string.IsNullOrWhiteSpace(x.Currency));

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("Cost must be greater than or equal to 0")
            .When(x => x.Cost.HasValue);

        RuleFor(x => x.Unit)
            .MaximumLength(50).WithMessage("Unit must not exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Unit));
    }
}

