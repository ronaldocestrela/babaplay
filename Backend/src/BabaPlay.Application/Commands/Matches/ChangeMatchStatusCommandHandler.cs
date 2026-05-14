using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Application.Commands.Matches;

public sealed class ChangeMatchStatusCommandHandler
    : ICommandHandler<ChangeMatchStatusCommand, Result<MatchResponse>>
{
    private readonly IMatchRepository _matchRepository;

    public ChangeMatchStatusCommandHandler(IMatchRepository matchRepository)
        => _matchRepository = matchRepository;

    public async Task<Result<MatchResponse>> HandleAsync(ChangeMatchStatusCommand cmd, CancellationToken ct = default)
    {
        var match = await _matchRepository.GetByIdAsync(cmd.MatchId, ct);
        if (match is null)
            return Result<MatchResponse>.Fail("MATCH_NOT_FOUND", $"Match '{cmd.MatchId}' was not found.");

        try
        {
            match.ChangeStatus(cmd.Status);
        }
        catch (ValidationException)
        {
            return Result<MatchResponse>.Fail("INVALID_STATUS_TRANSITION", "Invalid match status transition.");
        }

        await _matchRepository.UpdateAsync(match, ct);
        await _matchRepository.SaveChangesAsync(ct);

        return Result<MatchResponse>.Ok(new MatchResponse(
            match.Id,
            match.TenantId,
            match.GameDayId,
            match.HomeTeamId,
            match.AwayTeamId,
            match.Description,
            match.Status,
            match.IsActive,
            match.CreatedAt));
    }
}