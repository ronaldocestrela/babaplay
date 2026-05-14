using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

public sealed class PlayerScoreSourceEvent : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid SourceEventId { get; private set; }
    public Guid PlayerId { get; private set; }
    public DateTime AppliedAtUtc { get; private set; }

    private PlayerScoreSourceEvent() { }

    public static PlayerScoreSourceEvent Create(Guid tenantId, Guid sourceEventId, Guid playerId, DateTime appliedAtUtc)
    {
        if (tenantId == Guid.Empty)
            throw new ValidationException("TenantId", "TenantId is required.");

        if (sourceEventId == Guid.Empty)
            throw new ValidationException("SourceEventId", "SourceEventId is required.");

        if (playerId == Guid.Empty)
            throw new ValidationException("PlayerId", "PlayerId is required.");

        if (appliedAtUtc.Kind != DateTimeKind.Utc)
            throw new ValidationException("AppliedAtUtc", "AppliedAtUtc must be UTC.");

        return new PlayerScoreSourceEvent
        {
            TenantId = tenantId,
            SourceEventId = sourceEventId,
            PlayerId = playerId,
            AppliedAtUtc = appliedAtUtc,
        };
    }
}