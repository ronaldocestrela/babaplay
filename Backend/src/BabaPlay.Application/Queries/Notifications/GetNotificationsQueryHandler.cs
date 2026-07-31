using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Notifications;

public sealed class GetNotificationsQueryHandler
    : IQueryHandler<GetNotificationsQuery, Result<NotificationSummaryResponse>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ITenantContext _tenantContext;

    public GetNotificationsQueryHandler(
        INotificationRepository notificationRepository,
        ITenantContext tenantContext)
    {
        _notificationRepository = notificationRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<NotificationSummaryResponse>> HandleAsync(
        GetNotificationsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.UserId == Guid.Empty)
            return Result<NotificationSummaryResponse>.Fail("INVALID_USER", "UserId is required.");

        var tenantId = _tenantContext.TenantId;

        var notifications = await _notificationRepository.GetByUserAsync(
            tenantId,
            query.UserId,
            query.OnlyUnread,
            query.Limit,
            cancellationToken);

        var unreadCount = await _notificationRepository.GetUnreadCountAsync(
            tenantId,
            query.UserId,
            cancellationToken);

        var responses = notifications.Select(n => new NotificationResponse(
            n.Id,
            n.TenantId,
            n.UserId,
            n.Type,
            n.Title,
            n.Message,
            n.PayloadJson,
            n.IsRead,
            n.ReadAtUtc,
            n.CreatedAt
        )).ToList();

        var summary = new NotificationSummaryResponse(unreadCount, responses);

        return Result<NotificationSummaryResponse>.Ok(summary);
    }
}
