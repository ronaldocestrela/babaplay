using BabaPlay.Domain.Enums;
using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

/// <summary>
/// Represents a tenant-scoped match between two teams on a specific game day.
/// </summary>
public sealed class Match : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid GameDayId { get; private set; }
    public Guid HomeTeamId { get; private set; }
    public Guid AwayTeamId { get; private set; }
    public string? Description { get; private set; }
    public MatchStatus Status { get; private set; }
    public bool IsActive { get; private set; }

    private Match() { }

    public static Match Create(
        Guid tenantId,
        Guid gameDayId,
        Guid homeTeamId,
        Guid awayTeamId,
        string? description)
    {
        ValidateIds(tenantId, gameDayId, homeTeamId, awayTeamId);

        return new Match
        {
            TenantId = tenantId,
            GameDayId = gameDayId,
            HomeTeamId = homeTeamId,
            AwayTeamId = awayTeamId,
            Description = description?.Trim(),
            Status = MatchStatus.Pending,
            IsActive = true,
        };
    }

    public void Update(Guid gameDayId, Guid homeTeamId, Guid awayTeamId, string? description)
    {
        if (gameDayId == Guid.Empty)
            throw new ValidationException("GameDayId", "GameDayId is required.");

        if (homeTeamId == Guid.Empty)
            throw new ValidationException("HomeTeamId", "HomeTeamId is required.");

        if (awayTeamId == Guid.Empty)
            throw new ValidationException("AwayTeamId", "AwayTeamId is required.");

        if (homeTeamId == awayTeamId)
            throw new ValidationException("Teams", "Home and away teams must be different.");

        GameDayId = gameDayId;
        HomeTeamId = homeTeamId;
        AwayTeamId = awayTeamId;
        Description = description?.Trim();
        MarkUpdated();
    }

    public void ChangeStatus(MatchStatus newStatus)
    {
        if (Status == newStatus)
            return;

        var isValidTransition = Status switch
        {
            MatchStatus.Pending => newStatus is MatchStatus.Scheduled or MatchStatus.Cancelled,
            MatchStatus.Scheduled => newStatus is MatchStatus.InProgress or MatchStatus.Cancelled,
            MatchStatus.InProgress => newStatus is MatchStatus.Completed,
            MatchStatus.Completed => false,
            MatchStatus.Cancelled => false,
            _ => false,
        };

        if (!isValidTransition)
            throw new ValidationException("Status", "Invalid match status transition.");

        Status = newStatus;
        MarkUpdated();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        MarkUpdated();
    }

    private static void ValidateIds(Guid tenantId, Guid gameDayId, Guid homeTeamId, Guid awayTeamId)
    {
        if (tenantId == Guid.Empty)
            throw new ValidationException("TenantId", "TenantId is required.");

        if (gameDayId == Guid.Empty)
            throw new ValidationException("GameDayId", "GameDayId is required.");

        if (homeTeamId == Guid.Empty)
            throw new ValidationException("HomeTeamId", "HomeTeamId is required.");

        if (awayTeamId == Guid.Empty)
            throw new ValidationException("AwayTeamId", "AwayTeamId is required.");

        if (homeTeamId == awayTeamId)
            throw new ValidationException("Teams", "Home and away teams must be different.");
    }
}