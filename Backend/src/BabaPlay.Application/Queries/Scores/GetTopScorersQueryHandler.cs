using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.ValueObjects;

namespace BabaPlay.Application.Queries.Scores;

public sealed class GetTopScorersQueryHandler : IQueryHandler<GetTopScorersQuery, Result<IReadOnlyList<TopScorerEntryResponse>>>
{
    private readonly IPlayerScoreRepository _playerScoreRepository;

    public GetTopScorersQueryHandler(IPlayerScoreRepository playerScoreRepository)
        => _playerScoreRepository = playerScoreRepository;

    public async Task<Result<IReadOnlyList<TopScorerEntryResponse>>> HandleAsync(GetTopScorersQuery query, CancellationToken ct = default)
    {
        if (!TryBuildPeriod(query.FromUtc, query.ToUtc, out var period))
            return Result<IReadOnlyList<TopScorerEntryResponse>>.Fail("INVALID_PERIOD", "FromUtc and ToUtc must both be provided and valid UTC dates.");

        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;
        var skip = (page - 1) * pageSize;

        var scores = await _playerScoreRepository.GetTopScorersAsync(period, skip, pageSize, ct);

        return Result<IReadOnlyList<TopScorerEntryResponse>>.Ok(scores
            .Select((score, index) => new TopScorerEntryResponse(
                Rank: skip + index + 1,
                PlayerId: score.PlayerId,
                Goals: score.Goals,
                ScoreTotal: score.ScoreTotal))
            .ToList());
    }

    private static bool TryBuildPeriod(DateTime? fromUtc, DateTime? toUtc, out RankingPeriod? period)
    {
        period = null;

        if (!fromUtc.HasValue && !toUtc.HasValue)
            return true;

        if (!fromUtc.HasValue || !toUtc.HasValue)
            return false;

        try
        {
            period = RankingPeriod.Create(fromUtc.Value, toUtc.Value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}