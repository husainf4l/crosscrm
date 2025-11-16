using FluentValidation;

namespace crm_backend.Modules.Financial.DTOs.Validators;

public class CreateInvoiceDtoValidator : AbstractValidator<CreateInvoiceDto>
{
    public CreateInvoiceDtoValidator()
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
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-character code (e.g., USD)");

        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer ID is required");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow.Date).WithMessage("Due date must be in the future");

        RuleFor(x => x.LineItems)
            .NotEmpty().WithMessage("At least one line item is required");

        RuleForEach(x => x.LineItems)
            .SetValidator(new CreateInvoiceLineItemDtoValidator());
    }
}

public class CreateInvoiceLineItemDtoValidator : AbstractValidator<CreateInvoiceLineItemDto>
{
    public CreateInvoiceLineItemDtoValidator()
    {
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

