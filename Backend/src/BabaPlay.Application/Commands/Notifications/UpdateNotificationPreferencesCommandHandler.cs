using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Commands.Notifications;

public sealed class UpdateNotificationPreferencesCommandHandler
    : ICommandHandler<UpdateNotificationPreferencesCommand, Result<UserNotificationPreferencesResponse>>
{
    private readonly IUserNotificationPreferencesRepository _repository;
    private readonly ITenantContext _tenantContext;

    public UpdateNotificationPreferencesCommandHandler(
        IUserNotificationPreferencesRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<UserNotificationPreferencesResponse>> HandleAsync(UpdateNotificationPreferencesCommand cmd, CancellationToken ct = default)
    {
        if (cmd.UserId == Guid.Empty)
            return Result<UserNotificationPreferencesResponse>.Fail("NOTIFICATION_INVALID_USER_ID", "UserId is required.");

        var tenantId = _tenantContext.TenantId;
        var preferences = await _repository.GetByUserAsync(tenantId, cmd.UserId, ct);

        if (preferences is null)
        {
            preferences = UserNotificationPreferences.CreateDefault(tenantId, cmd.UserId);
            preferences.Update(cmd.PushEnabled, cmd.CheckinEnabled, cmd.MatchEnabled, cmd.MatchEventEnabled, cmd.GameDayEnabled);
            await _repository.AddAsync(preferences, ct);
            await _repository.SaveChangesAsync(ct);
            return Result<UserNotificationPreferencesResponse>.Ok(ToResponse(preferences));
        }

        preferences.Update(cmd.PushEnabled, cmd.CheckinEnabled, cmd.MatchEnabled, cmd.MatchEventEnabled, cmd.GameDayEnabled);
        await _repository.UpdateAsync(preferences, ct);
        await _repository.SaveChangesAsync(ct);
        return Result<UserNotificationPreferencesResponse>.Ok(ToResponse(preferences));
    }

    private static UserNotificationPreferencesResponse ToResponse(UserNotificationPreferences preferences) => new(
        preferences.Id,
        preferences.TenantId,
        preferences.UserId,
        preferences.PushEnabled,
        preferences.CheckinEnabled,
        preferences.MatchEnabled,
        preferences.MatchEventEnabled,
        preferences.GameDayEnabled,
        preferences.IsActive);
}
