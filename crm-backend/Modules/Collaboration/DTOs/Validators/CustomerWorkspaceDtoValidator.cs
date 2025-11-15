using FluentValidation;
using crm_backend.Modules.Collaboration;
using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.DTOs.Validators;

public class CreateCustomerWorkspaceDtoValidator : AbstractValidator<CreateCustomerWorkspaceDto>
{
    public CreateCustomerWorkspaceDtoValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer ID is required");
    }
}

public class UpdateCustomerWorkspaceDtoValidator : AbstractValidator<UpdateCustomerWorkspaceDto>
{
    public UpdateCustomerWorkspaceDtoValidator()
    {
        RuleFor(x => x.Summary)
            .MaximumLength(5000).WithMessage("Summary must not exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.Summary));
    }
}

public class CreateCustomerStrategyDtoValidator : AbstractValidator<CreateCustomerStrategyDto>
{
    public CreateCustomerStrategyDtoValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Strategy type is required")
            .Must(BeValidStrategyType).WithMessage("Invalid strategy type. Must be one of: Engagement, Upsell, Retention, WinBack");

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Priority is required")
            .Must(BeValidPriority).WithMessage("Invalid priority. Must be one of: Low, Medium, High, Critical");

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }

    private bool BeValidStrategyType(string type)
    {
        return Enum.TryParse<StrategyType>(type, true, out _);
    }

    private bool BeValidPriority(string priority)
    {
        return Enum.TryParse<StrategyPriority>(priority, true, out _);
    }
}

public class UpdateCustomerStrategyDtoValidator : AbstractValidator<UpdateCustomerStrategyDto>
{
    public UpdateCustomerStrategyDtoValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Type)
            .Must(BeValidStrategyType).WithMessage("Invalid strategy type. Must be one of: Engagement, Upsell, Retention, WinBack")
            .When(x => !string.IsNullOrEmpty(x.Type));

        RuleFor(x => x.Status)
            .Must(BeValidStatus).WithMessage("Invalid status. Must be one of: Draft, Active, Completed, Cancelled")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.Priority)
            .Must(BeValidPriority).WithMessage("Invalid priority. Must be one of: Low, Medium, High, Critical")
            .When(x => !string.IsNullOrEmpty(x.Priority));

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }

    private bool BeValidStrategyType(string? type)
    {
        if (string.IsNullOrEmpty(type))
            return true;
        return Enum.TryParse<StrategyType>(type, true, out _);
    }

    private bool BeValidStatus(string? status)
    {
        if (string.IsNullOrEmpty(status))
            return true;
        return Enum.TryParse<StrategyStatus>(status, true, out _);
    }

    private bool BeValidPriority(string? priority)
    {
        if (string.IsNullOrEmpty(priority))
            return true;
        return Enum.TryParse<StrategyPriority>(priority, true, out _);
    }
}

public class CreateCustomerIdeaDtoValidator : AbstractValidator<CreateCustomerIdeaDto>
{
    public CreateCustomerIdeaDtoValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .Must(BeValidCategory).WithMessage("Invalid category. Must be one of: Product, Service, Process, Marketing");

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Priority is required")
            .Must(BeValidPriority).WithMessage("Invalid priority. Must be one of: Low, Medium, High, Critical");

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }

    private bool BeValidCategory(string category)
    {
        return Enum.TryParse<IdeaCategory>(category, true, out _);
    }

    private bool BeValidPriority(string priority)
    {
        return Enum.TryParse<IdeaPriority>(priority, true, out _);
    }
}

public class UpdateCustomerIdeaDtoValidator : AbstractValidator<UpdateCustomerIdeaDto>
{
    public UpdateCustomerIdeaDtoValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Category)
            .Must(BeValidCategory).WithMessage("Invalid category. Must be one of: Product, Service, Process, Marketing")
            .When(x => !string.IsNullOrEmpty(x.Category));

        RuleFor(x => x.Status)
            .Must(BeValidStatus).WithMessage("Invalid status. Must be one of: New, UnderReview, Approved, Rejected, Implemented")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.Priority)
            .Must(BeValidPriority).WithMessage("Invalid priority. Must be one of: Low, Medium, High, Critical")
            .When(x => !string.IsNullOrEmpty(x.Priority));

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }

    private bool BeValidCategory(string? category)
    {
        if (string.IsNullOrEmpty(category))
            return true;
        return Enum.TryParse<IdeaCategory>(category, true, out _);
    }

    private bool BeValidStatus(string? status)
    {
        if (string.IsNullOrEmpty(status))
            return true;
        return Enum.TryParse<IdeaStatus>(status, true, out _);
    }

    private bool BeValidPriority(string? priority)
    {
        if (string.IsNullOrEmpty(priority))
            return true;
        return Enum.TryParse<IdeaPriority>(priority, true, out _);
    }
}

public class CreateNoteCommentDtoValidator : AbstractValidator<CreateNoteCommentDto>
{
    public CreateNoteCommentDtoValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("Entity type is required")
            .Must(BeValidEntityType).WithMessage("Invalid entity type. Must be one of: Note, Strategy, Idea");

        RuleFor(x => x.EntityId)
            .GreaterThan(0).WithMessage("Entity ID is required");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .MaximumLength(5000).WithMessage("Content must not exceed 5000 characters");
    }

    private bool BeValidEntityType(string entityType)
    {
        return entityType.Equals("Note", StringComparison.OrdinalIgnoreCase) ||
               entityType.Equals("Strategy", StringComparison.OrdinalIgnoreCase) ||
               entityType.Equals("Idea", StringComparison.OrdinalIgnoreCase);
    }
}

public class UpdateNoteCommentDtoValidator : AbstractValidator<UpdateNoteCommentDto>
{
    public UpdateNoteCommentDtoValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .MaximumLength(5000).WithMessage("Content must not exceed 5000 characters");
    }
}

public class VoteIdeaDtoValidator : AbstractValidator<VoteIdeaDto>
{
    public VoteIdeaDtoValidator()
    {
        RuleFor(x => x.IdeaId)
            .GreaterThan(0).WithMessage("Idea ID is required");
    }
}

