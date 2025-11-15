using FluentValidation;
using crm_backend.Modules.Collaboration;
using crm_backend.Modules.Collaboration.DTOs;

namespace crm_backend.Modules.Collaboration.DTOs.Validators;

public class CreateAIAgentApiKeyDtoValidator : AbstractValidator<CreateAIAgentApiKeyDto>
{
    public CreateAIAgentApiKeyDtoValidator()
    {
        RuleFor(x => x.AgentId)
            .GreaterThan(0).WithMessage("Agent ID is required");

        RuleFor(x => x.KeyName)
            .NotEmpty().WithMessage("Key name is required")
            .MaximumLength(100).WithMessage("Key name must not exceed 100 characters");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future")
            .When(x => x.ExpiresAt.HasValue);

        RuleFor(x => x.RateLimitPerMinute)
            .GreaterThan(0).WithMessage("Rate limit per minute must be greater than 0")
            .When(x => x.RateLimitPerMinute.HasValue);

        RuleFor(x => x.RateLimitPerHour)
            .GreaterThan(0).WithMessage("Rate limit per hour must be greater than 0")
            .When(x => x.RateLimitPerHour.HasValue);
    }
}

public class UpdateAIAgentApiKeyDtoValidator : AbstractValidator<UpdateAIAgentApiKeyDto>
{
    public UpdateAIAgentApiKeyDtoValidator()
    {
        RuleFor(x => x.KeyName)
            .MaximumLength(100).WithMessage("Key name must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.KeyName));

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future")
            .When(x => x.ExpiresAt.HasValue);

        RuleFor(x => x.RateLimitPerMinute)
            .GreaterThan(0).WithMessage("Rate limit per minute must be greater than 0")
            .When(x => x.RateLimitPerMinute.HasValue);

        RuleFor(x => x.RateLimitPerHour)
            .GreaterThan(0).WithMessage("Rate limit per hour must be greater than 0")
            .When(x => x.RateLimitPerHour.HasValue);
    }
}

public class CreateAIAgentToolDtoValidator : AbstractValidator<CreateAIAgentToolDto>
{
    public CreateAIAgentToolDtoValidator()
    {
        RuleFor(x => x.AgentId)
            .GreaterThan(0).WithMessage("Agent ID is required");

        RuleFor(x => x.ToolName)
            .NotEmpty().WithMessage("Tool name is required")
            .MaximumLength(100).WithMessage("Tool name must not exceed 100 characters")
            .Matches("^[a-z0-9_]+$").WithMessage("Tool name must contain only lowercase letters, numbers, and underscores");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required")
            .MaximumLength(200).WithMessage("Display name must not exceed 200 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Tool type is required")
            .Must(BeValidToolType).WithMessage("Invalid tool type. Must be one of: Read, Write, Delete, Query, Action");

        RuleFor(x => x.Endpoint)
            .NotEmpty().WithMessage("Endpoint is required")
            .MaximumLength(500).WithMessage("Endpoint must not exceed 500 characters");

        RuleFor(x => x.Method)
            .NotEmpty().WithMessage("Method is required")
            .Must(BeValidHttpMethod).WithMessage("Invalid HTTP method. Must be one of: GET, POST, PUT, DELETE, PATCH");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }

    private bool BeValidToolType(string type)
    {
        return Enum.TryParse<ToolType>(type, true, out _);
    }

    private bool BeValidHttpMethod(string method)
    {
        var validMethods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" };
        return validMethods.Contains(method.ToUpper());
    }
}

public class UpdateAIAgentToolDtoValidator : AbstractValidator<UpdateAIAgentToolDto>
{
    public UpdateAIAgentToolDtoValidator()
    {
        RuleFor(x => x.DisplayName)
            .MaximumLength(200).WithMessage("Display name must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.DisplayName));

        RuleFor(x => x.Type)
            .Must(BeValidToolType).WithMessage("Invalid tool type. Must be one of: Read, Write, Delete, Query, Action")
            .When(x => !string.IsNullOrEmpty(x.Type));

        RuleFor(x => x.Endpoint)
            .MaximumLength(500).WithMessage("Endpoint must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Endpoint));

        RuleFor(x => x.Method)
            .Must(BeValidHttpMethod).WithMessage("Invalid HTTP method. Must be one of: GET, POST, PUT, DELETE, PATCH")
            .When(x => !string.IsNullOrEmpty(x.Method));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }

    private bool BeValidToolType(string? type)
    {
        if (string.IsNullOrEmpty(type))
            return true;
        return Enum.TryParse<ToolType>(type, true, out _);
    }

    private bool BeValidHttpMethod(string? method)
    {
        if (string.IsNullOrEmpty(method))
            return true;
        var validMethods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" };
        return validMethods.Contains(method.ToUpper());
    }
}

public class ExecuteToolDtoValidator : AbstractValidator<ExecuteToolDto>
{
    public ExecuteToolDtoValidator()
    {
        RuleFor(x => x.ToolName)
            .NotEmpty().WithMessage("Tool name is required");
    }
}

