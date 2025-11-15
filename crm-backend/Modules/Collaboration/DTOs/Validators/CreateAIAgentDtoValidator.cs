using FluentValidation;
using crm_backend.Modules.Collaboration;
using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.DTOs.Validators;

public class CreateAIAgentDtoValidator : AbstractValidator<CreateAIAgentDto>
{
    public CreateAIAgentDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Agent name is required")
            .MaximumLength(100).WithMessage("Agent name must not exceed 100 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Agent type is required")
            .Must(BeValidAgentType).WithMessage("Invalid agent type. Must be one of: CustomerService, SalesAssistant, DataAnalyst, StrategyAdvisor");

        RuleFor(x => x.SystemPrompt)
            .NotEmpty().WithMessage("System prompt is required")
            .MaximumLength(5000).WithMessage("System prompt must not exceed 5000 characters");

        RuleFor(x => x.Tools)
            .NotNull().WithMessage("Tools list is required")
            .Must(tools => tools != null && tools.Count > 0).WithMessage("At least one tool must be specified");

        RuleForEach(x => x.Tools)
            .NotEmpty().WithMessage("Tool name cannot be empty")
            .MaximumLength(50).WithMessage("Tool name must not exceed 50 characters");

        RuleFor(x => x.PythonServiceUrl)
            .Must(BeValidUrl).WithMessage("Python service URL must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.PythonServiceUrl));
    }

    private bool BeValidAgentType(string type)
    {
        return Enum.TryParse<AgentType>(type, true, out _);
    }

    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}

public class UpdateAIAgentDtoValidator : AbstractValidator<UpdateAIAgentDto>
{
    public UpdateAIAgentDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Agent name must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Type)
            .Must(BeValidAgentType).WithMessage("Invalid agent type. Must be one of: CustomerService, SalesAssistant, DataAnalyst, StrategyAdvisor")
            .When(x => !string.IsNullOrEmpty(x.Type));

        RuleFor(x => x.Status)
            .Must(BeValidAgentStatus).WithMessage("Invalid agent status. Must be one of: Active, Inactive, Training")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.SystemPrompt)
            .MaximumLength(5000).WithMessage("System prompt must not exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.SystemPrompt));

        RuleForEach(x => x.Tools)
            .NotEmpty().WithMessage("Tool name cannot be empty")
            .MaximumLength(50).WithMessage("Tool name must not exceed 50 characters")
            .When(x => x.Tools != null);

        RuleFor(x => x.PythonServiceUrl)
            .Must(BeValidUrl).WithMessage("Python service URL must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.PythonServiceUrl));
    }

    private bool BeValidAgentType(string? type)
    {
        if (string.IsNullOrEmpty(type))
            return true;
        return Enum.TryParse<AgentType>(type, true, out _);
    }

    private bool BeValidAgentStatus(string? status)
    {
        if (string.IsNullOrEmpty(status))
            return true;
        return Enum.TryParse<AgentStatus>(status, true, out _);
    }

    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}

public class CreateAIAgentAssignmentDtoValidator : AbstractValidator<CreateAIAgentAssignmentDto>
{
    public CreateAIAgentAssignmentDtoValidator()
    {
        RuleFor(x => x.AgentId)
            .GreaterThan(0).WithMessage("Agent ID is required");

        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("Entity type is required")
            .MaximumLength(50).WithMessage("Entity type must not exceed 50 characters");

        RuleFor(x => x.EntityId)
            .GreaterThan(0).WithMessage("Entity ID is required");
    }
}

public class CreateAIAgentInteractionDtoValidator : AbstractValidator<CreateAIAgentInteractionDto>
{
    public CreateAIAgentInteractionDtoValidator()
    {
        RuleFor(x => x.AgentId)
            .GreaterThan(0).WithMessage("Agent ID is required");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Interaction type is required")
            .Must(BeValidInteractionType).WithMessage("Invalid interaction type. Must be one of: Query, Action, Analysis, Suggestion, Message");

        RuleFor(x => x.Input)
            .NotEmpty().WithMessage("Input is required")
            .MaximumLength(10000).WithMessage("Input must not exceed 10000 characters");

        RuleFor(x => x.EntityType)
            .MaximumLength(50).WithMessage("Entity type must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.EntityType));
    }

    private bool BeValidInteractionType(string type)
    {
        return Enum.TryParse<InteractionType>(type, true, out _);
    }
}

