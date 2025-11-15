using FluentValidation;

namespace crm_backend.Modules.Financial.DTOs.Validators;

public class CreatePaymentDtoValidator : AbstractValidator<CreatePaymentDto>
{
    public CreatePaymentDtoValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than 0");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-character code (e.g., USD)");

        RuleFor(x => x.InvoiceId)
            .GreaterThan(0).WithMessage("Invoice ID is required");

        RuleFor(x => x.TransactionId)
            .MaximumLength(200).WithMessage("Transaction ID must not exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.TransactionId));

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(200).WithMessage("Reference number must not exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.ReferenceNumber));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}

