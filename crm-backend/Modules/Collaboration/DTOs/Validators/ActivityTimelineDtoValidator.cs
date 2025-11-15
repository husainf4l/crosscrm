using FluentValidation;
using crm_backend.Modules.Collaboration;
using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.DTOs.Validators;

public class CreateActivityTimelineDtoValidator : AbstractValidator<CreateActivityTimelineDto>
{
    public CreateActivityTimelineDtoValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("Entity type is required")
            .MaximumLength(50).WithMessage("Entity type must not exceed 50 characters");

        RuleFor(x => x.EntityId)
            .GreaterThan(0).WithMessage("Entity ID is required");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Activity type is required")
            .Must(BeValidActivityType).WithMessage("Invalid activity type");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }

    private bool BeValidActivityType(string type)
    {
        return Enum.TryParse<ActivityType>(type, true, out _);
    }
}

