namespace BabaPlay.Application.DTOs;

public sealed record CheckinResponse(
    Guid Id,
    Guid TenantId,
    Guid PlayerId,
    Guid GameDayId,
    DateTime CheckedInAtUtc,
    double Latitude,
    double Longitude,
    double DistanceFromAssociationMeters,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? CancelledAtUtc);
