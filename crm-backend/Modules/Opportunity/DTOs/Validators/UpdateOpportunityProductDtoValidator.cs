using FluentValidation;

namespace crm_backend.Modules.Opportunity.DTOs.Validators;

public class UpdateOpportunityProductDtoValidator : AbstractValidator<UpdateOpportunityProductDto>
{
    public UpdateOpportunityProductDtoValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0")
            .When(x => x.Quantity.HasValue);

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0")
            .When(x => x.UnitPrice.HasValue);

        RuleFor(x => x.DiscountPercent)
            .InclusiveBetween(0, 100).WithMessage("Discount percent must be between 0 and 100")
            .When(x => x.DiscountPercent.HasValue);
    }
}

