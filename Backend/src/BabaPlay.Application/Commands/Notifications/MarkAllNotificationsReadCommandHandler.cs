using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Notifications;

public sealed class MarkAllNotificationsReadCommandHandler
    : ICommandHandler<MarkAllNotificationsReadCommand, Result>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ITenantContext _tenantContext;

    public MarkAllNotificationsReadCommandHandler(
        INotificationRepository notificationRepository,
        ITenantContext tenantContext)
    {
        _notificationRepository = notificationRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result> HandleAsync(
        MarkAllNotificationsReadCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.UserId == Guid.Empty)
            return Result.Fail("INVALID_USER", "UserId is required.");

        var tenantId = _tenantContext.TenantId;
        await _notificationRepository.MarkAllAsReadAsync(tenantId, command.UserId, cancellationToken);

        return Result.Ok();
    }
}
