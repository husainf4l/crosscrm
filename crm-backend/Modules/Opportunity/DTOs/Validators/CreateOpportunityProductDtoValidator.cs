using FluentValidation;

namespace crm_backend.Modules.Opportunity.DTOs.Validators;

public class CreateOpportunityProductDtoValidator : AbstractValidator<CreateOpportunityProductDto>
{
    public CreateOpportunityProductDtoValidator()
    {
        RuleFor(x => x.OpportunityId)
            .GreaterThan(0).WithMessage("Opportunity ID is required");

        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0");

        RuleFor(x => x.DiscountPercent)
            .InclusiveBetween(0, 100).WithMessage("Discount percent must be between 0 and 100")
            .When(x => x.DiscountPercent.HasValue);
    }
}

