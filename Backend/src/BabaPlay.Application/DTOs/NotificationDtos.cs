using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.DTOs;

/// <summary>
/// Response DTO representing a user notification.
/// </summary>
public sealed record NotificationResponse(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    NotificationType Type,
    string Title,
    string Message,
    string? PayloadJson,
    bool IsRead,
    DateTime? ReadAtUtc,
    DateTime CreatedAt);

/// <summary>
/// Response DTO containing unread count and a list of recent notifications.
/// </summary>
public sealed record NotificationSummaryResponse(
    int UnreadCount,
    IReadOnlyList<NotificationResponse> Notifications);

/// <summary>
/// Response DTO after an admin sends notifications to tenant members.
/// </summary>
public sealed record SendNotificationResponse(int CreatedCount);
