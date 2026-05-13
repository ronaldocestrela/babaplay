using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Matches;

public sealed class UpdateMatchCommandHandler
    : ICommandHandler<UpdateMatchCommand, Result<MatchResponse>>
{
    private readonly IMatchRepository _matchRepository;
    private readonly IGameDayRepository _gameDayRepository;
    private readonly ITeamRepository _teamRepository;

    public UpdateMatchCommandHandler(
        IMatchRepository matchRepository,
        IGameDayRepository gameDayRepository,
        ITeamRepository teamRepository)
    {
        _matchRepository = matchRepository;
        _gameDayRepository = gameDayRepository;
        _teamRepository = teamRepository;
    }

    public async Task<Result<MatchResponse>> HandleAsync(UpdateMatchCommand cmd, CancellationToken ct = default)
    {
        var match = await _matchRepository.GetByIdAsync(cmd.MatchId, ct);
        if (match is null)
            return Result<MatchResponse>.Fail("MATCH_NOT_FOUND", $"Match '{cmd.MatchId}' was not found.");

        if (cmd.GameDayId == Guid.Empty)
            return Result<MatchResponse>.Fail("INVALID_GAMEDAY_ID", "GameDayId is required.");

        var homeTeamId = cmd.HomeTeamId ?? Guid.Empty;
        var awayTeamId = cmd.AwayTeamId ?? Guid.Empty;
        var hasFixedTeams = homeTeamId != Guid.Empty || awayTeamId != Guid.Empty;

        if ((homeTeamId == Guid.Empty) != (awayTeamId == Guid.Empty))
            return Result<MatchResponse>.Fail("MATCH_TEAMS_PAIR_REQUIRED", "Home and away teams must be both provided or both empty.");

        if (hasFixedTeams && homeTeamId == awayTeamId)
            return Result<MatchResponse>.Fail("TEAMS_MUST_BE_DIFFERENT", "Home and away teams must be different.");

        var gameDay = await _gameDayRepository.GetByIdAsync(cmd.GameDayId, ct);
        if (gameDay is null)
            return Result<MatchResponse>.Fail("GAMEDAY_NOT_FOUND", $"Game day '{cmd.GameDayId}' was not found.");

        if (gameDay.ScheduledAt <= DateTime.UtcNow)
            return Result<MatchResponse>.Fail("GAMEDAY_PAST", "Cannot update match for a past game day.");

        bool exists;
        if (hasFixedTeams)
        {
            var homeTeam = await _teamRepository.GetByIdAsync(homeTeamId, ct);
            var awayTeam = await _teamRepository.GetByIdAsync(awayTeamId, ct);
            if (homeTeam is null || awayTeam is null || !homeTeam.IsActive || !awayTeam.IsActive)
                return Result<MatchResponse>.Fail("TEAM_NOT_FOUND", "One or both teams were not found.");

            exists = await _matchRepository.ExistsByGameDayAndTeamsAsync(
                cmd.GameDayId,
                homeTeamId,
                awayTeamId,
                cmd.MatchId,
                ct);
        }
        else
        {
            exists = await _matchRepository.ExistsByGameDayAsync(cmd.GameDayId, cmd.MatchId, ct);
        }

        if (exists)
            return Result<MatchResponse>.Fail("MATCH_ALREADY_EXISTS", "A match for the same game day already exists.");

        match.Update(cmd.GameDayId, homeTeamId, awayTeamId, cmd.Description);

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