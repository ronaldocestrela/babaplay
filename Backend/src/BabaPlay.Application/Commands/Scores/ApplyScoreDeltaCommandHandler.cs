using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using BabaPlay.Domain.ValueObjects;

namespace BabaPlay.Application.Commands.Scores;

public sealed class ApplyScoreDeltaCommandHandler : ICommandHandler<ApplyScoreDeltaCommand, Result>
{
    private readonly IPlayerScoreRepository _playerScoreRepository;
    private readonly ITenantContext _tenantContext;

    public ApplyScoreDeltaCommandHandler(
        IPlayerScoreRepository playerScoreRepository,
        ITenantContext tenantContext)
    {
        _playerScoreRepository = playerScoreRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result> HandleAsync(ApplyScoreDeltaCommand cmd, CancellationToken ct = default)
    {
        if (cmd.SourceEventId == Guid.Empty)
            return Result.Fail("INVALID_SOURCE_EVENT_ID", "SourceEventId is required.");

        if (cmd.PlayerId == Guid.Empty)
            return Result.Fail("INVALID_PLAYER_ID", "PlayerId is required.");

        var alreadyProcessed = await _playerScoreRepository.HasProcessedSourceEventAsync(cmd.SourceEventId, ct);
        if (alreadyProcessed)
            return Result.Fail("DUPLICATE_SCORE_EVENT", "The source event was already processed.");

        var delta = new ScoreBreakdown(
            AttendanceCount: cmd.AttendanceDelta,
            Wins: cmd.WinsDelta,
            Draws: cmd.DrawsDelta,
            Goals: cmd.GoalsDelta,
            YellowCards: cmd.YellowCardsDelta,
            RedCards: cmd.RedCardsDelta);

        var score = await _playerScoreRepository.GetByPlayerIdAsync(cmd.PlayerId, ct);
        var isCreating = score is null;

        if (isCreating)
        {
            if (WouldGenerateNegativeCounters(delta))
                return Result.Fail("PLAYER_SCORE_NOT_FOUND", "Player score was not found for a negative delta operation.");

            score = PlayerScore.Create(_tenantContext.TenantId, cmd.PlayerId);
        }

        try
        {
            score.ApplyDelta(delta);
        }
        catch (ValidationException)
        {
            return Result.Fail("INVALID_SCORE_DELTA", "Delta would generate invalid score counters.");
        }

        if (isCreating)
            await _playerScoreRepository.AddAsync(score, ct);
        else
            await _playerScoreRepository.UpdateAsync(score, ct);

        var sourceEvent = PlayerScoreSourceEvent.Create(
            _tenantContext.TenantId,
            cmd.SourceEventId,
            cmd.PlayerId,
            DateTime.UtcNow);

        await _playerScoreRepository.AddProcessedSourceEventAsync(sourceEvent, ct);
        await _playerScoreRepository.SaveChangesAsync(ct);

        return Result.Ok();
    }

    private static bool WouldGenerateNegativeCounters(ScoreBreakdown delta)
        => delta.AttendanceCount < 0
            || delta.Wins < 0
            || delta.Draws < 0
            || delta.Goals < 0
            || delta.YellowCards < 0
            || delta.RedCards < 0;
}