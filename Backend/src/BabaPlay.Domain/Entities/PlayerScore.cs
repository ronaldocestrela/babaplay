using BabaPlay.Domain.Exceptions;
using BabaPlay.Domain.ValueObjects;

namespace BabaPlay.Domain.Entities;

public sealed class PlayerScore : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid PlayerId { get; private set; }

    public int AttendanceCount { get; private set; }
    public int Wins { get; private set; }
    public int Draws { get; private set; }
    public int Goals { get; private set; }
    public int YellowCards { get; private set; }
    public int RedCards { get; private set; }

    public int ScoreTotal { get; private set; }
    public bool IsActive { get; private set; }

    private PlayerScore() { }

    public static PlayerScore Create(Guid tenantId, Guid playerId)
    {
        if (tenantId == Guid.Empty)
            throw new ValidationException("TenantId", "TenantId is required.");

        if (playerId == Guid.Empty)
            throw new ValidationException("PlayerId", "PlayerId is required.");

        return new PlayerScore
        {
            TenantId = tenantId,
            PlayerId = playerId,
            AttendanceCount = 0,
            Wins = 0,
            Draws = 0,
            Goals = 0,
            YellowCards = 0,
            RedCards = 0,
            ScoreTotal = 0,
            IsActive = true,
        };
    }

    public ScoreBreakdown GetBreakdown()
        => new(AttendanceCount, Wins, Draws, Goals, YellowCards, RedCards);

    public void ReplaceBreakdown(ScoreBreakdown breakdown)
    {
        EnsureCounterNotNegative("AttendanceCount", breakdown.AttendanceCount);
        EnsureCounterNotNegative("Wins", breakdown.Wins);
        EnsureCounterNotNegative("Draws", breakdown.Draws);
        EnsureCounterNotNegative("Goals", breakdown.Goals);
        EnsureCounterNotNegative("YellowCards", breakdown.YellowCards);
        EnsureCounterNotNegative("RedCards", breakdown.RedCards);

        AttendanceCount = breakdown.AttendanceCount;
        Wins = breakdown.Wins;
        Draws = breakdown.Draws;
        Goals = breakdown.Goals;
        YellowCards = breakdown.YellowCards;
        RedCards = breakdown.RedCards;

        RecomputeScore();
        MarkUpdated();
    }

    public void ApplyDelta(ScoreBreakdown delta)
    {
        var attendance = AttendanceCount + delta.AttendanceCount;
        var wins = Wins + delta.Wins;
        var draws = Draws + delta.Draws;
        var goals = Goals + delta.Goals;
        var yellowCards = YellowCards + delta.YellowCards;
        var redCards = RedCards + delta.RedCards;

        EnsureCounterNotNegative("AttendanceCount", attendance);
        EnsureCounterNotNegative("Wins", wins);
        EnsureCounterNotNegative("Draws", draws);
        EnsureCounterNotNegative("Goals", goals);
        EnsureCounterNotNegative("YellowCards", yellowCards);
        EnsureCounterNotNegative("RedCards", redCards);

        AttendanceCount = attendance;
        Wins = wins;
        Draws = draws;
        Goals = goals;
        YellowCards = yellowCards;
        RedCards = redCards;

        RecomputeScore();
        MarkUpdated();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        MarkUpdated();
    }

    private void RecomputeScore()
        => ScoreTotal = GetBreakdown().CalculateTotal();

    private static void EnsureCounterNotNegative(string field, int value)
    {
        if (value < 0)
            throw new ValidationException(field, $"{field} cannot be negative.");
    }
}