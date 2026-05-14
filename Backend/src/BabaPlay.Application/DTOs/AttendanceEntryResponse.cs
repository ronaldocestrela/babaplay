namespace BabaPlay.Application.DTOs;

public sealed record AttendanceEntryResponse(
    int Rank,
    Guid PlayerId,
    int AttendanceCount,
    int ScoreTotal);