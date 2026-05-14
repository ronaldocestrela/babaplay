using BabaPlay.Domain.Exceptions;
using BabaPlay.Domain.ValueObjects;

namespace BabaPlay.Domain.Entities;

public sealed class Checkin : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid PlayerId { get; private set; }
    public Guid GameDayId { get; private set; }
    public DateTime CheckedInAtUtc { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public double DistanceFromAssociationMeters { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }

    private Checkin() { }

    public static Checkin Create(
        Guid tenantId,
        Guid playerId,
        Guid gameDayId,
        DateTime checkedInAtUtc,
        double latitude,
        double longitude,
        double distanceFromAssociationMeters)
    {
        if (tenantId == Guid.Empty)
            throw new ValidationException("TenantId", "TenantId is required.");

        if (playerId == Guid.Empty)
            throw new ValidationException("PlayerId", "PlayerId is required.");

        if (gameDayId == Guid.Empty)
            throw new ValidationException("GameDayId", "GameDayId is required.");

        _ = GeoCoordinate.Create(latitude, longitude);

        if (distanceFromAssociationMeters < 0)
            throw new ValidationException("DistanceFromAssociationMeters", "Distance must be greater than or equal to zero.");

        return new Checkin
        {
            TenantId = tenantId,
            PlayerId = playerId,
            GameDayId = gameDayId,
            CheckedInAtUtc = checkedInAtUtc,
            Latitude = latitude,
            Longitude = longitude,
            DistanceFromAssociationMeters = distanceFromAssociationMeters,
            IsActive = true,
            CancelledAtUtc = null,
        };
    }

    public void Deactivate(DateTime cancelledAtUtc)
    {
        if (!IsActive)
            return;

        IsActive = false;
        CancelledAtUtc = cancelledAtUtc;
        MarkUpdated();
    }
}
