using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.ValueObjects;

namespace BabaPlay.Application.Commands.Scores;

public sealed class RebuildTenantRankingCommandHandler
    : ICommandHandler<RebuildTenantRankingCommand, Result<RebuildRankingResponse>>
{
    private readonly IPlayerScoreRepository _playerScoreRepository;

    public RebuildTenantRankingCommandHandler(IPlayerScoreRepository playerScoreRepository)
        => _playerScoreRepository = playerScoreRepository;

    public async Task<Result<RebuildRankingResponse>> HandleAsync(RebuildTenantRankingCommand cmd, CancellationToken ct = default)
    {
        if (!TryBuildPeriod(cmd.FromUtc, cmd.ToUtc, out var period))
            return Result<RebuildRankingResponse>.Fail("INVALID_PERIOD", "FromUtc and ToUtc must both be provided and valid UTC dates.");

        try
        {
            var scores = await _playerScoreRepository.GetAllActiveForRebuildAsync(period, ct);

            foreach (var score in scores)
            {
                score.ReplaceBreakdown(score.GetBreakdown());
                await _playerScoreRepository.UpdateAsync(score, ct);
            }

            await _playerScoreRepository.SaveChangesAsync(ct);

            return Result<RebuildRankingResponse>.Ok(new RebuildRankingResponse(
                ProcessedCount: scores.Count,
                RebuiltAtUtc: DateTime.UtcNow));
        }
        catch
        {
            return Result<RebuildRankingResponse>.Fail("RANKING_REBUILD_FAILED", "Failed to rebuild ranking snapshot.");
        }
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