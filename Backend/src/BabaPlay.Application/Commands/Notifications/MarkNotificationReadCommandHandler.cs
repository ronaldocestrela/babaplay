using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Notifications;

public sealed class MarkNotificationReadCommandHandler
    : ICommandHandler<MarkNotificationReadCommand, Result>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ITenantContext _tenantContext;

    public MarkNotificationReadCommandHandler(
        INotificationRepository notificationRepository,
        ITenantContext tenantContext)
    {
        _notificationRepository = notificationRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result> HandleAsync(
        MarkNotificationReadCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.NotificationId == Guid.Empty)
            return Result.Fail("INVALID_NOTIFICATION", "NotificationId is required.");

        if (command.UserId == Guid.Empty)
            return Result.Fail("INVALID_USER", "UserId is required.");

        var tenantId = _tenantContext.TenantId;
        var notification = await _notificationRepository.GetByIdAsync(
            command.NotificationId,
            tenantId,
            cancellationToken);

        if (notification is null || notification.UserId != command.UserId)
            return Result.Fail("NOT_FOUND", "Notification not found or does not belong to the user.");

        if (notification.IsRead)
            return Result.Ok();

        notification.MarkAsRead(DateTime.UtcNow);
        await _notificationRepository.UpdateAsync(notification, cancellationToken);

        return Result.Ok();
    }
}
