namespace BabaPlay.Application.DTOs;

public sealed record MatchSummaryResponse(
    Guid Id,
    Guid TenantId,
    Guid MatchId,
    string FileName,
    string ContentType,
    long SizeBytes,
    DateTime GeneratedAtUtc,
    bool IsActive);
