using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.ValueObjects;

namespace BabaPlay.Application.Queries.Scores;

public sealed class GetRankingQueryHandler : IQueryHandler<GetRankingQuery, Result<IReadOnlyList<RankingEntryResponse>>>
{
    private readonly IPlayerScoreRepository _playerScoreRepository;

    public GetRankingQueryHandler(IPlayerScoreRepository playerScoreRepository)
        => _playerScoreRepository = playerScoreRepository;

    public async Task<Result<IReadOnlyList<RankingEntryResponse>>> HandleAsync(GetRankingQuery query, CancellationToken ct = default)
    {
        if (!TryBuildPeriod(query.FromUtc, query.ToUtc, out var period))
            return Result<IReadOnlyList<RankingEntryResponse>>.Fail("INVALID_PERIOD", "FromUtc and ToUtc must both be provided and valid UTC dates.");

        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;
        var skip = (page - 1) * pageSize;

        var scores = await _playerScoreRepository.GetRankingAsync(period, skip, pageSize, ct);

        return Result<IReadOnlyList<RankingEntryResponse>>.Ok(scores
            .Select((score, index) => new RankingEntryResponse(
                Rank: skip + index + 1,
                PlayerId: score.PlayerId,
                ScoreTotal: score.ScoreTotal,
                AttendanceCount: score.AttendanceCount,
                Wins: score.Wins,
                Draws: score.Draws,
                Goals: score.Goals,
                YellowCards: score.YellowCards,
                RedCards: score.RedCards))
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