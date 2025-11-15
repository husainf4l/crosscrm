using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Subscriptions;
using HotChocolate.Execution;
using crm_backend.Modules.Collaboration.DTOs;
using crm_backend.Modules.Collaboration.Services;
using crm_backend.Data;
using crm_backend.GraphQL;
using crm_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using FluentValidation;

namespace crm_backend.Modules.Collaboration;

[ExtendObjectType(typeof(crm_backend.GraphQL.Query))]
public class CommunicationResolver : BaseResolver
{
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<ChannelDto>> GetChannels(
        [Service] IChannelService channelService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var userId = GetUserId(httpContextAccessor.HttpContext);
        return await channelService.GetChannelsByCompanyAsync(companyId, userId);
    }

    [Authorize]
    public async Task<ChannelDto?> GetChannel(
        int id,
        [Service] IChannelService channelService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var userId = GetUserId(httpContextAccessor.HttpContext);
        return await channelService.GetChannelByIdAsync(id, userId);
    }

    [Authorize]
    public async Task<IEnumerable<ChannelMemberDto>> GetChannelMembers(
        int channelId,
        [Service] IChannelService channelService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        return await channelService.GetChannelMembersAsync(channelId, companyId);
    }

    [Authorize]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<MessageDto>> GetMessages(
        int channelId,
        [Service] IMessageService messageService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        int skip = 0,
        int take = 50)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        var userId = GetUserId(httpContextAccessor.HttpContext);
        return await messageService.GetMessagesByChannelAsync(channelId, companyId, userId, skip, take);
    }

    [Authorize]
    public async Task<IEnumerable<MessageDto>> GetThreadReplies(
        int parentMessageId,
        [Service] IMessageService messageService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context)
    {
        var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
        return await messageService.GetThreadRepliesAsync(parentMessageId, companyId);
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Mutation))]
public class CommunicationMutation : BaseResolver
{
    [Authorize]
    public async Task<ChannelDto> CreateChannel(
        CreateChannelDto input,
        [Service] IChannelService channelService,
        [Service] ITopicEventSender eventSender,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateChannelDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                throw new GraphQLException(errorHandling.CreateValidationError(validationResult));
            }

            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            var userId = GetUserId(httpContextAccessor.HttpContext);
            var channel = await channelService.CreateChannelAsync(input, companyId, userId);

            // Publish event for subscription
            await eventSender.SendAsync($"channel_created_{companyId}", channel);

            return channel;
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("CREATE_CHANNEL_ERROR", $"Failed to create channel: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<MessageDto> SendMessage(
        CreateMessageDto input,
        [Service] IMessageService messageService,
        [Service] ITopicEventSender eventSender,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<CreateMessageDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                throw new GraphQLException(errorHandling.CreateValidationError(validationResult));
            }

            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            var userId = GetUserId(httpContextAccessor.HttpContext);
            var message = await messageService.SendMessageAsync(input, companyId, userId);

            // Publish event for subscription - channel-specific
            await eventSender.SendAsync($"message_created_{input.ChannelId}", message);
            // Also publish to company-wide for notifications
            await eventSender.SendAsync($"message_created_company_{companyId}", message);

            return message;
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("SEND_MESSAGE_ERROR", $"Failed to send message: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<MessageDto?> UpdateMessage(
        int id,
        UpdateMessageDto input,
        [Service] IMessageService messageService,
        [Service] ITopicEventSender eventSender,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<UpdateMessageDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                throw new GraphQLException(errorHandling.CreateValidationError(validationResult));
            }

            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            var userId = GetUserId(httpContextAccessor.HttpContext);
            var message = await messageService.UpdateMessageAsync(id, input, companyId, userId);

            if (message != null)
            {
                // Publish event for subscription
                await eventSender.SendAsync($"message_updated_{message.ChannelId}", message);
            }

            return message;
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("UPDATE_MESSAGE_ERROR", $"Failed to update message: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<MessageDto> AddReaction(
        AddReactionDto input,
        [Service] IMessageService messageService,
        [Service] ITopicEventSender eventSender,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<AddReactionDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                throw new GraphQLException(errorHandling.CreateValidationError(validationResult));
            }

            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            var userId = GetUserId(httpContextAccessor.HttpContext);
            var message = await messageService.AddReactionAsync(input, userId, companyId);

            // Publish event for subscription
            await eventSender.SendAsync($"message_updated_{message.ChannelId}", message);

            return message;
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("ADD_REACTION_ERROR", $"Failed to add reaction: {ex.Message}"));
        }
    }

    [Authorize]
    public async Task<bool> MarkChannelAsRead(
        MarkChannelReadDto input,
        [Service] IChannelService channelService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] CrmDbContext context,
        [Service] IValidator<MarkChannelReadDto> validator,
        [Service] IErrorHandlingService errorHandling)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                throw new GraphQLException(errorHandling.CreateValidationError(validationResult));
            }

            var companyId = await GetActiveCompanyIdAsync(httpContextAccessor, context);
            var userId = GetUserId(httpContextAccessor.HttpContext);
            return await channelService.MarkChannelAsReadAsync(input.ChannelId, userId, companyId);
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(errorHandling.CreateError("MARK_READ_ERROR", $"Failed to mark channel as read: {ex.Message}"));
        }
    }
}

[ExtendObjectType(typeof(crm_backend.GraphQL.Subscription))]
public class CommunicationSubscription : BaseResolver
{
    [Authorize]
    [Subscribe(With = nameof(OnMessageCreatedAsync))]
    public MessageDto OnMessageCreated(
        [EventMessage] MessageDto message)
    {
        return message;
    }

    public async ValueTask<ISourceStream<MessageDto>> OnMessageCreatedAsync(
        int channelId,
        [Service] ITopicEventReceiver eventReceiver,
        CancellationToken cancellationToken)
    {
        return await eventReceiver.SubscribeAsync<MessageDto>($"message_created_{channelId}", cancellationToken);
    }

    [Authorize]
    [Subscribe(With = nameof(OnMessageUpdatedAsync))]
    public MessageDto OnMessageUpdated(
        [EventMessage] MessageDto message)
    {
        return message;
    }

    public async ValueTask<ISourceStream<MessageDto>> OnMessageUpdatedAsync(
        int channelId,
        [Service] ITopicEventReceiver eventReceiver,
        CancellationToken cancellationToken)
    {
        return await eventReceiver.SubscribeAsync<MessageDto>($"message_updated_{channelId}", cancellationToken);
    }

    [Authorize]
    [Subscribe(With = nameof(OnChannelCreatedAsync))]
    public ChannelDto OnChannelCreated(
        [EventMessage] ChannelDto channel)
    {
        return channel;
    }

    public async ValueTask<ISourceStream<ChannelDto>> OnChannelCreatedAsync(
        int companyId,
        [Service] ITopicEventReceiver eventReceiver,
        CancellationToken cancellationToken)
    {
        return await eventReceiver.SubscribeAsync<ChannelDto>($"channel_created_{companyId}", cancellationToken);
    }
}

