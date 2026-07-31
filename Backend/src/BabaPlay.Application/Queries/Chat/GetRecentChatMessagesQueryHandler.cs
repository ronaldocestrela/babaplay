using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Chat;

public sealed class GetRecentChatMessagesQueryHandler
    : IQueryHandler<GetRecentChatMessagesQuery, Result<IReadOnlyList<ChatMessageResponse>>>
{
    private readonly IChatMessageRepository _chatRepository;
    private readonly ITenantContext _tenantContext;

    public GetRecentChatMessagesQueryHandler(
        IChatMessageRepository chatRepository,
        ITenantContext tenantContext)
    {
        _chatRepository = chatRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<ChatMessageResponse>>> HandleAsync(
        GetRecentChatMessagesQuery query,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        var limit = Math.Clamp(query.Limit, 1, 200);

        var messages = await _chatRepository.GetRecentMessagesAsync(tenantId, limit, cancellationToken);

        var responses = messages.Select(m => new ChatMessageResponse(
            m.Id,
            m.TenantId,
            m.SenderId,
            m.SenderName,
            m.Content,
            m.SentAtUtc
        )).ToList();

        return Result<IReadOnlyList<ChatMessageResponse>>.Ok(responses);
    }
}
