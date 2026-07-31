using System;

namespace BabaPlay.Web.Models;

public sealed record ChatMessageDto(
    Guid Id,
    Guid TenantId,
    Guid SenderId,
    string SenderName,
    string Content,
    DateTime SentAtUtc);

public sealed record SendChatMessageDto(string Content);
