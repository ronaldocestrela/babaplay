using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Notifications;

public sealed record GetNotificationsQuery(
    Guid UserId,
    bool OnlyUnread = false,
    int Limit = 20) : IQuery<Result<NotificationSummaryResponse>>;
