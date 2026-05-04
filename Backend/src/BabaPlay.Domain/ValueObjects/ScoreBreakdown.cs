namespace BabaPlay.Domain.ValueObjects;

public readonly record struct ScoreBreakdown(
    int AttendanceCount,
    int Wins,
    int Draws,
    int Goals,
    int YellowCards,
    int RedCards)
{
    public const int AttendancePoints = 1;
    public const int WinPoints = 3;
    public const int DrawPoints = 1;
    public const int GoalPoints = 2;
    public const int YellowCardPenalty = -1;
    public const int RedCardPenalty = -3;

    public static ScoreBreakdown Zero => new(0, 0, 0, 0, 0, 0);

    public int CalculateTotal()
        => (AttendanceCount * AttendancePoints)
            + (Wins * WinPoints)
            + (Draws * DrawPoints)
            + (Goals * GoalPoints)
            + (YellowCards * YellowCardPenalty)
            + (RedCards * RedCardPenalty);
}