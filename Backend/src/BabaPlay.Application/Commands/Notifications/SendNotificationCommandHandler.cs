using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Commands.Notifications;

public sealed class SendNotificationCommandHandler
    : ICommandHandler<SendNotificationCommand, Result<SendNotificationResponse>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserTenantRepository _userTenantRepository;
    private readonly ITenantContext _tenantContext;

    public SendNotificationCommandHandler(
        INotificationRepository notificationRepository,
        IUserTenantRepository userTenantRepository,
        ITenantContext tenantContext)
    {
        _notificationRepository = notificationRepository;
        _userTenantRepository = userTenantRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<SendNotificationResponse>> HandleAsync(
        SendNotificationCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            return Result<SendNotificationResponse>.Fail("INVALID_TITLE", "Title is required.");

        if (string.IsNullOrWhiteSpace(command.Message))
            return Result<SendNotificationResponse>.Fail("INVALID_MESSAGE", "Message is required.");

        var tenantId = _tenantContext.TenantId;
        if (tenantId == Guid.Empty)
            return Result<SendNotificationResponse>.Fail("INVALID_TENANT", "Tenant context is not resolved.");

        var memberUserIds = await _userTenantRepository.GetMemberUserIdsAsync(tenantId, cancellationToken);
        if (memberUserIds.Count == 0)
            return Result<SendNotificationResponse>.Fail("NO_RECIPIENTS", "No tenant members found to notify.");

        IReadOnlyList<Guid> recipients;
        if (command.TargetUserIds is { Count: > 0 })
        {
            var memberSet = memberUserIds.ToHashSet();
            recipients = command.TargetUserIds
                .Where(id => id != Guid.Empty && memberSet.Contains(id))
                .Distinct()
                .ToList();

            if (recipients.Count == 0)
                return Result<SendNotificationResponse>.Fail("NO_RECIPIENTS", "No valid target users found in this tenant.");
        }
        else
        {
            recipients = memberUserIds;
        }

        var notifications = recipients
            .Select(userId => Notification.Create(
                tenantId,
                userId,
                command.Type,
                command.Title,
                command.Message,
                payloadJson: null))
            .ToList();

        await _notificationRepository.AddRangeAsync(notifications, cancellationToken);

        return Result<SendNotificationResponse>.Ok(new SendNotificationResponse(notifications.Count));
    }
}
