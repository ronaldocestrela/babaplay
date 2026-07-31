using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Application.Commands.Chat;

public sealed class SendChatMessageCommandHandler
    : ICommandHandler<SendChatMessageCommand, Result<ChatMessageResponse>>
{
    private readonly IChatMessageRepository _chatRepository;
    private readonly ITenantContext _tenantContext;

    public SendChatMessageCommandHandler(
        IChatMessageRepository chatRepository,
        ITenantContext tenantContext)
    {
        _chatRepository = chatRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<ChatMessageResponse>> HandleAsync(
        SendChatMessageCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.SenderId == Guid.Empty)
            return Result<ChatMessageResponse>.Fail("INVALID_SENDER", "SenderId is required.");

        if (string.IsNullOrWhiteSpace(command.SenderName))
            return Result<ChatMessageResponse>.Fail("INVALID_SENDER_NAME", "SenderName is required.");

        if (string.IsNullOrWhiteSpace(command.Content))
            return Result<ChatMessageResponse>.Fail("INVALID_CONTENT", "Content is required.");

        var tenantId = _tenantContext.TenantId;
        if (tenantId == Guid.Empty)
            return Result<ChatMessageResponse>.Fail("INVALID_TENANT", "Tenant context is not resolved.");

        try
        {
            var message = ChatMessage.Create(
                tenantId,
                command.SenderId,
                command.SenderName,
                command.Content);

            await _chatRepository.AddAsync(message, cancellationToken);

            var response = new ChatMessageResponse(
                message.Id,
                message.TenantId,
                message.SenderId,
                message.SenderName,
                message.Content,
                message.SentAtUtc);

            return Result<ChatMessageResponse>.Ok(response);
        }
        catch (ValidationException ex)
        {
            return Result<ChatMessageResponse>.Fail("VALIDATION_ERROR", ex.Message);
        }
    }
}
