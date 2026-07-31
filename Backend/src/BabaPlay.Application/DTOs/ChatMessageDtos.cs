namespace BabaPlay.Application.DTOs;

/// <summary>
/// Response DTO representing a chat message.
/// </summary>
public sealed record ChatMessageResponse(
    Guid Id,
    Guid TenantId,
    Guid SenderId,
    string SenderName,
    string Content,
    DateTime SentAtUtc);

/// <summary>
/// Request payload to send a new chat message.
/// </summary>
public sealed record SendChatMessageRequest(string Content);
