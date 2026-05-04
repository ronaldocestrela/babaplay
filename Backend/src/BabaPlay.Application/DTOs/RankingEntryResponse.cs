namespace BabaPlay.Application.DTOs;

public sealed record RankingEntryResponse(
    int Rank,
    Guid PlayerId,
    int ScoreTotal,
    int AttendanceCount,
    int Wins,
    int Draws,
    int Goals,
    int YellowCards,
    int RedCards);