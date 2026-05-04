using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.ValueObjects;

namespace BabaPlay.Application.Queries.Scores;

public sealed class GetAttendanceRankingQueryHandler : IQueryHandler<GetAttendanceRankingQuery, Result<IReadOnlyList<AttendanceEntryResponse>>>
{
    private readonly IPlayerScoreRepository _playerScoreRepository;

    public GetAttendanceRankingQueryHandler(IPlayerScoreRepository playerScoreRepository)
        => _playerScoreRepository = playerScoreRepository;

    public async Task<Result<IReadOnlyList<AttendanceEntryResponse>>> HandleAsync(GetAttendanceRankingQuery query, CancellationToken ct = default)
    {
        if (!TryBuildPeriod(query.FromUtc, query.ToUtc, out var period))
            return Result<IReadOnlyList<AttendanceEntryResponse>>.Fail("INVALID_PERIOD", "FromUtc and ToUtc must both be provided and valid UTC dates.");

        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;
        var skip = (page - 1) * pageSize;

        var scores = await _playerScoreRepository.GetAttendanceRankingAsync(period, skip, pageSize, ct);

        return Result<IReadOnlyList<AttendanceEntryResponse>>.Ok(scores
            .Select((score, index) => new AttendanceEntryResponse(
                Rank: skip + index + 1,
                PlayerId: score.PlayerId,
                AttendanceCount: score.AttendanceCount,
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