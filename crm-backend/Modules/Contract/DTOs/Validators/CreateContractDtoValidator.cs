using FluentValidation;

namespace crm_backend.Modules.Contract.DTOs.Validators;

public class CreateContractDtoValidator : AbstractValidator<CreateContractDto>
{
    public CreateContractDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Contract name is required")
            .MaximumLength(200).WithMessage("Contract name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

        RuleFor(x => x.RenewalDate)
            .GreaterThan(x => x.EndDate).WithMessage("Renewal date must be after end date")
            .When(x => x.RenewalDate.HasValue && x.EndDate != default);

        RuleFor(x => x.TotalValue)
            .GreaterThanOrEqualTo(0).WithMessage("Total value must be greater than or equal to 0")
            .When(x => x.TotalValue.HasValue);

        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer ID is required");

        RuleFor(x => x.LineItems)
            .Must(items => items == null || items.Count == 0 || items.All(i => i.Quantity > 0))
            .WithMessage("All line items must have a quantity greater than 0")
            .When(x => x.LineItems != null && x.LineItems.Count > 0);
    }
}

public class UpdateContractDtoValidator : AbstractValidator<UpdateContractDto>
{
    public UpdateContractDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Contract name must not exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.RenewalDate)
            .GreaterThan(x => x.EndDate).WithMessage("Renewal date must be after end date")
            .When(x => x.RenewalDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.TotalValue)
            .GreaterThanOrEqualTo(0).WithMessage("Total value must be greater than or equal to 0")
            .When(x => x.TotalValue.HasValue);
    }
}

