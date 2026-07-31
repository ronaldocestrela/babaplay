using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Chat;

public sealed record GetRecentChatMessagesQuery(
    int Limit = 50) : IQuery<Result<IReadOnlyList<ChatMessageResponse>>>;
