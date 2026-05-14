namespace BabaPlay.Application.DTOs;

public sealed record RebuildRankingResponse(
    int ProcessedCount,
    DateTime RebuiltAtUtc);