using System;
using System.Collections.Generic;

namespace BabaPlay.Web.Models;

public sealed record NotificationDto(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    int Type,
    string Title,
    string Message,
    string? PayloadJson,
    bool IsRead,
    DateTime? ReadAtUtc,
    DateTime CreatedAt);

public sealed record NotificationSummaryDto(
    int UnreadCount,
    List<NotificationDto> Notifications);
