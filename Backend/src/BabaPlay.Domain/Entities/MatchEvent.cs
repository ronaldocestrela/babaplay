using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

/// <summary>
/// Represents a tenant-scoped event registered for a match timeline.
/// </summary>
public sealed class MatchEvent : EntityBase
{
    public const int MaxMinute = 130;

    public Guid TenantId { get; private set; }
    public Guid MatchId { get; private set; }
    public Guid TeamId { get; private set; }
    public Guid PlayerId { get; private set; }
    public Guid MatchEventTypeId { get; private set; }
    public int Minute { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }

    private MatchEvent() { }

    public static MatchEvent Create(
        Guid tenantId,
        Guid matchId,
        Guid teamId,
        Guid playerId,
        Guid matchEventTypeId,
        int minute,
        string? notes)
    {
        ValidateIds(tenantId, matchId, teamId, playerId, matchEventTypeId);
        ValidateMinute(minute);

        return new MatchEvent
        {
            TenantId = tenantId,
            MatchId = matchId,
            TeamId = teamId,
            PlayerId = playerId,
            MatchEventTypeId = matchEventTypeId,
            Minute = minute,
            Notes = notes?.Trim(),
            IsActive = true,
        };
    }

    public void Update(Guid matchEventTypeId, int minute, string? notes)
    {
        if (matchEventTypeId == Guid.Empty)
            throw new ValidationException("MatchEventTypeId", "MatchEventTypeId is required.");

        ValidateMinute(minute);

        MatchEventTypeId = matchEventTypeId;
        Minute = minute;
        Notes = notes?.Trim();
        MarkUpdated();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        MarkUpdated();
    }

    private static void ValidateIds(
        Guid tenantId,
        Guid matchId,
        Guid teamId,
        Guid playerId,
        Guid matchEventTypeId)
    {
        if (tenantId == Guid.Empty)
            throw new ValidationException("TenantId", "TenantId is required.");

        if (matchId == Guid.Empty)
            throw new ValidationException("MatchId", "MatchId is required.");

        if (teamId == Guid.Empty)
            throw new ValidationException("TeamId", "TeamId is required.");

        if (playerId == Guid.Empty)
            throw new ValidationException("PlayerId", "PlayerId is required.");

        if (matchEventTypeId == Guid.Empty)
            throw new ValidationException("MatchEventTypeId", "MatchEventTypeId is required.");
    }

    private static void ValidateMinute(int minute)
    {
        if (minute < 0 || minute > MaxMinute)
            throw new ValidationException("Minute", $"Minute must be between 0 and {MaxMinute}.");
    }
}
