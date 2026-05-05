using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Commands.Notifications;

public sealed class RegisterDeviceTokenCommandHandler
    : ICommandHandler<RegisterDeviceTokenCommand, Result<DeviceTokenResponse>>
{
    private readonly IUserDeviceTokenRepository _repository;
    private readonly ITenantContext _tenantContext;

    public RegisterDeviceTokenCommandHandler(
        IUserDeviceTokenRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<DeviceTokenResponse>> HandleAsync(RegisterDeviceTokenCommand cmd, CancellationToken ct = default)
    {
        if (cmd.UserId == Guid.Empty)
            return Result<DeviceTokenResponse>.Fail("NOTIFICATION_INVALID_USER_ID", "UserId is required.");

        if (string.IsNullOrWhiteSpace(cmd.DeviceId))
            return Result<DeviceTokenResponse>.Fail("NOTIFICATION_INVALID_DEVICE_ID", "DeviceId is required.");

        if (string.IsNullOrWhiteSpace(cmd.Token))
            return Result<DeviceTokenResponse>.Fail("NOTIFICATION_INVALID_TOKEN", "Token is required.");

        if (string.IsNullOrWhiteSpace(cmd.Platform))
            return Result<DeviceTokenResponse>.Fail("NOTIFICATION_INVALID_PLATFORM", "Platform is required.");

        var tenantId = _tenantContext.TenantId;
        var normalizedDeviceId = cmd.DeviceId.Trim();

        var existing = await _repository.GetByUserAndDeviceAsync(tenantId, cmd.UserId, normalizedDeviceId, ct);

        if (existing is null)
        {
            var created = UserDeviceToken.Create(tenantId, cmd.UserId, normalizedDeviceId, cmd.Token, cmd.Platform);
            await _repository.AddAsync(created, ct);
            await _repository.SaveChangesAsync(ct);
            return Result<DeviceTokenResponse>.Ok(ToResponse(created));
        }

        existing.RotateToken(cmd.Token);
        await _repository.UpdateAsync(existing, ct);
        await _repository.SaveChangesAsync(ct);
        return Result<DeviceTokenResponse>.Ok(ToResponse(existing));
    }

    private static DeviceTokenResponse ToResponse(UserDeviceToken token) => new(
        token.Id,
        token.TenantId,
        token.UserId,
        token.DeviceId,
        token.Token,
        token.Platform,
        token.IsActive);
}
