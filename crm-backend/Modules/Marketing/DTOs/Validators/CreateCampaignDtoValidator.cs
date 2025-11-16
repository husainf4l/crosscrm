using FluentValidation;

namespace crm_backend.Modules.Marketing.DTOs.Validators;

public class CreateCampaignDtoValidator : AbstractValidator<CreateCampaignDto>
{
    public CreateCampaignDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Campaign name is required")
            .MaximumLength(200).WithMessage("Campaign name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Budget)
            .GreaterThanOrEqualTo(0).WithMessage("Budget must be greater than or equal to 0")
            .When(x => x.Budget.HasValue);

        RuleFor(x => x.ExpectedLeads)
            .GreaterThanOrEqualTo(0).WithMessage("Expected leads must be greater than or equal to 0")
            .When(x => x.ExpectedLeads.HasValue);

        RuleFor(x => x.ExpectedRevenue)
            .GreaterThanOrEqualTo(0).WithMessage("Expected revenue must be greater than or equal to 0")
            .When(x => x.ExpectedRevenue.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}

public class UpdateCampaignDtoValidator : AbstractValidator<UpdateCampaignDto>
{
    public UpdateCampaignDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Campaign name must not exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Budget)
            .GreaterThanOrEqualTo(0).WithMessage("Budget must be greater than or equal to 0")
            .When(x => x.Budget.HasValue);

        RuleFor(x => x.ActualCost)
            .GreaterThanOrEqualTo(0).WithMessage("Actual cost must be greater than or equal to 0")
            .When(x => x.ActualCost.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}

