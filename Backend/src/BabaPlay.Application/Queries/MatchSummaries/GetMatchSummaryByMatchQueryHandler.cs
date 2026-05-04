using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Queries.MatchSummaries;

public sealed class GetMatchSummaryByMatchQueryHandler
    : IQueryHandler<GetMatchSummaryByMatchQuery, Result<MatchSummaryResponse>>
{
    private readonly IMatchSummaryRepository _summaryRepository;

    public GetMatchSummaryByMatchQueryHandler(IMatchSummaryRepository summaryRepository)
        => _summaryRepository = summaryRepository;

    public async Task<Result<MatchSummaryResponse>> HandleAsync(GetMatchSummaryByMatchQuery query, CancellationToken ct = default)
    {
        var summary = await _summaryRepository.GetByMatchIdAsync(query.MatchId, ct);
        if (summary is null)
            return Result<MatchSummaryResponse>.Fail("MATCH_SUMMARY_NOT_FOUND", $"Match summary for match '{query.MatchId}' was not found.");

        return Result<MatchSummaryResponse>.Ok(ToResponse(summary));
    }

    private static MatchSummaryResponse ToResponse(MatchSummary summary) => new(
        summary.Id,
        summary.TenantId,
        summary.MatchId,
        summary.FileName,
        summary.ContentType,
        summary.SizeBytes,
        summary.GeneratedAtUtc,
        summary.IsActive);
}
