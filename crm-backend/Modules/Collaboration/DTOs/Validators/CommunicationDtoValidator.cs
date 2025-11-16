using FluentValidation;

namespace crm_backend.Modules.Collaboration.DTOs.Validators;

public class CreateChannelDtoValidator : AbstractValidator<CreateChannelDto>
{
    public CreateChannelDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Channel name is required")
            .MaximumLength(100).WithMessage("Channel name must not exceed 100 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Channel type is required")
            .Must(BeValidChannelType).WithMessage("Invalid channel type. Must be one of: Public, Private, Direct, Customer");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }

    private bool BeValidChannelType(string type)
    {
        return Enum.TryParse<ChannelType>(type, true, out _);
    }
}

public class UpdateChannelDtoValidator : AbstractValidator<UpdateChannelDto>
{
    public UpdateChannelDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Channel name must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

public class AddChannelMemberDtoValidator : AbstractValidator<AddChannelMemberDto>
{
    public AddChannelMemberDtoValidator()
    {
        RuleFor(x => x.ChannelId)
            .GreaterThan(0).WithMessage("Channel ID is required");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID is required");

        RuleFor(x => x.Role)
            .Must(BeValidRole).WithMessage("Invalid role. Must be one of: Admin, Member, ReadOnly")
            .When(x => !string.IsNullOrEmpty(x.Role));
    }

    private bool BeValidRole(string? role)
    {
        if (string.IsNullOrEmpty(role))
            return true;
        return Enum.TryParse<ChannelMemberRole>(role, true, out _);
    }
}

public class CreateMessageDtoValidator : AbstractValidator<CreateMessageDto>
{
    public CreateMessageDtoValidator()
    {
        RuleFor(x => x.ChannelId)
            .GreaterThan(0).WithMessage("Channel ID is required");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Message content is required")
            .MaximumLength(10000).WithMessage("Message content must not exceed 10000 characters");

        RuleFor(x => x.ContentType)
            .Must(BeValidContentType).WithMessage("Invalid content type. Must be one of: Text, Markdown, RichText")
            .When(x => !string.IsNullOrEmpty(x.ContentType));
    }

    private bool BeValidContentType(string contentType)
    {
        return Enum.TryParse<MessageContentType>(contentType, true, out _);
    }
}

public class UpdateMessageDtoValidator : AbstractValidator<UpdateMessageDto>
{
    public UpdateMessageDtoValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Message content is required")
            .MaximumLength(10000).WithMessage("Message content must not exceed 10000 characters");
    }
}

public class AddReactionDtoValidator : AbstractValidator<AddReactionDto>
{
    public AddReactionDtoValidator()
    {
        RuleFor(x => x.MessageId)
            .GreaterThan(0).WithMessage("Message ID is required");

        RuleFor(x => x.Reaction)
            .NotEmpty().WithMessage("Reaction is required")
            .MaximumLength(10).WithMessage("Reaction must not exceed 10 characters");
    }
}

public class RemoveReactionDtoValidator : AbstractValidator<RemoveReactionDto>
{
    public RemoveReactionDtoValidator()
    {
        RuleFor(x => x.MessageId)
            .GreaterThan(0).WithMessage("Message ID is required");

        RuleFor(x => x.Reaction)
            .NotEmpty().WithMessage("Reaction is required");
    }
}

public class MarkChannelReadDtoValidator : AbstractValidator<MarkChannelReadDto>
{
    public MarkChannelReadDtoValidator()
    {
        RuleFor(x => x.ChannelId)
            .GreaterThan(0).WithMessage("Channel ID is required");
    }
}

