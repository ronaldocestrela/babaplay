namespace BabaPlay.Application.Interfaces;

public interface IMatchSummaryPdfGenerator
{
    Task<byte[]> GenerateAsync(MatchSummaryPdfInput input, CancellationToken ct = default);
}

public sealed record MatchSummaryPdfInput(
    Guid MatchId,
    Guid GameDayId,
    Guid HomeTeamId,
    Guid AwayTeamId,
    string? Description,
    DateTime GeneratedAtUtc,
    IReadOnlyList<MatchSummaryPdfEventItem> Events);

public sealed record MatchSummaryPdfEventItem(
    Guid TeamId,
    Guid PlayerId,
    Guid MatchEventTypeId,
    int Minute,
    string? Notes);
