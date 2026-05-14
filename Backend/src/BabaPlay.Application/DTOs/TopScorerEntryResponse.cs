namespace BabaPlay.Application.DTOs;

public sealed record TopScorerEntryResponse(
    int Rank,
    Guid PlayerId,
    int Goals,
    int ScoreTotal);