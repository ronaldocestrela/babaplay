using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.ValueObjects;

namespace BabaPlay.Application.Commands.Checkins;

public sealed class CreateCheckinCommandHandler
    : ICommandHandler<CreateCheckinCommand, Result<CheckinResponse>>
{
    private readonly ICheckinRepository _checkinRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IGameDayRepository _gameDayRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ITenantGeolocationSettingsRepository _tenantGeolocationSettingsRepository;
    private readonly ICheckinRealtimeNotifier _checkinRealtimeNotifier;

    public CreateCheckinCommandHandler(
        ICheckinRepository checkinRepository,
        IPlayerRepository playerRepository,
        IGameDayRepository gameDayRepository,
        ITenantContext tenantContext,
        ITenantGeolocationSettingsRepository tenantGeolocationSettingsRepository,
        ICheckinRealtimeNotifier checkinRealtimeNotifier)
    {
        _checkinRepository = checkinRepository;
        _playerRepository = playerRepository;
        _gameDayRepository = gameDayRepository;
        _tenantContext = tenantContext;
        _tenantGeolocationSettingsRepository = tenantGeolocationSettingsRepository;
        _checkinRealtimeNotifier = checkinRealtimeNotifier;
    }

    public async Task<Result<CheckinResponse>> HandleAsync(CreateCheckinCommand cmd, CancellationToken ct = default)
    {
        var player = await _playerRepository.GetByIdAsync(cmd.PlayerId, ct);
        if (player is null)
            return Result<CheckinResponse>.Fail("PLAYER_NOT_FOUND", "Player was not found.");

        if (!player.IsActive)
            return Result<CheckinResponse>.Fail("PLAYER_INACTIVE", "Player is inactive.");

        var gameDay = await _gameDayRepository.GetByIdAsync(cmd.GameDayId, ct);
        if (gameDay is null)
            return Result<CheckinResponse>.Fail("GAMEDAY_NOT_FOUND", "Game day was not found.");

        if (cmd.CheckedInAtUtc.Date != gameDay.ScheduledAt.Date)
        {
            await _checkinRealtimeNotifier.NotifyCheckinDeniedAsync(cmd.GameDayId, cmd.PlayerId, "CHECKIN_DAY_INVALID", ct);
            return Result<CheckinResponse>.Fail("CHECKIN_DAY_INVALID", "Check-in is allowed only on the game day date.");
        }

        var geoSettings = await _tenantGeolocationSettingsRepository.GetSettingsAsync(_tenantContext.TenantId, ct);
        if (geoSettings is null)
            return Result<CheckinResponse>.Fail("ASSOCIATION_GEOLOCATION_NOT_CONFIGURED", "Association geolocation settings were not configured.");

        var associationCoordinate = GeoCoordinate.Create(geoSettings.Latitude, geoSettings.Longitude);
        var checkinCoordinate = GeoCoordinate.Create(cmd.Latitude, cmd.Longitude);
        var distanceMeters = associationCoordinate.DistanceToMeters(checkinCoordinate);

        if (distanceMeters > geoSettings.CheckinRadiusMeters)
        {
            await _checkinRealtimeNotifier.NotifyCheckinDeniedAsync(cmd.GameDayId, cmd.PlayerId, "CHECKIN_OUTSIDE_ALLOWED_RADIUS", ct);
            return Result<CheckinResponse>.Fail("CHECKIN_OUTSIDE_ALLOWED_RADIUS", "Player is outside the allowed check-in radius.");
        }

        var exists = await _checkinRepository.ExistsActiveByPlayerAndGameDayAsync(cmd.PlayerId, cmd.GameDayId, ct);
        if (exists)
        {
            await _checkinRealtimeNotifier.NotifyCheckinDeniedAsync(cmd.GameDayId, cmd.PlayerId, "CHECKIN_ALREADY_EXISTS", ct);
            return Result<CheckinResponse>.Fail("CHECKIN_ALREADY_EXISTS", "Player already checked in for this game day.");
        }

        var checkin = Checkin.Create(
            _tenantContext.TenantId,
            cmd.PlayerId,
            cmd.GameDayId,
            cmd.CheckedInAtUtc,
            cmd.Latitude,
            cmd.Longitude,
            distanceMeters);

        await _checkinRepository.AddAsync(checkin, ct);
        await _checkinRepository.SaveChangesAsync(ct);

        var activeCount = await _checkinRepository.CountActiveByGameDayAsync(cmd.GameDayId, ct);
        await _checkinRealtimeNotifier.NotifyCheckinCreatedAsync(cmd.GameDayId, cmd.PlayerId, ct);
        await _checkinRealtimeNotifier.NotifyCheckinCountUpdatedAsync(cmd.GameDayId, activeCount, ct);

        return Result<CheckinResponse>.Ok(ToResponse(checkin));
    }

    private static CheckinResponse ToResponse(Checkin checkin) => new(
        checkin.Id,
        checkin.TenantId,
        checkin.PlayerId,
        checkin.GameDayId,
        checkin.CheckedInAtUtc,
        checkin.Latitude,
        checkin.Longitude,
        checkin.DistanceFromAssociationMeters,
        checkin.IsActive,
        checkin.CreatedAt,
        checkin.CancelledAtUtc);
}
