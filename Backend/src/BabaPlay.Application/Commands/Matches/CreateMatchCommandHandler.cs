using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Commands.Matches;

public sealed class CreateMatchCommandHandler
    : ICommandHandler<CreateMatchCommand, Result<MatchResponse>>
{
    private readonly IMatchRepository _matchRepository;
    private readonly IGameDayRepository _gameDayRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ITenantContext _tenantContext;

    public CreateMatchCommandHandler(
        IMatchRepository matchRepository,
        IGameDayRepository gameDayRepository,
        ITeamRepository teamRepository,
        ITenantContext tenantContext)
    {
        _matchRepository = matchRepository;
        _gameDayRepository = gameDayRepository;
        _teamRepository = teamRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<MatchResponse>> HandleAsync(CreateMatchCommand cmd, CancellationToken ct = default)
    {
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
            return Result<MatchResponse>.Fail("GAMEDAY_PAST", "Cannot create match for a past game day.");

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
                null,
                ct);
        }
        else
        {
            exists = await _matchRepository.ExistsByGameDayAsync(cmd.GameDayId, null, ct);
        }

        if (exists)
            return Result<MatchResponse>.Fail("MATCH_ALREADY_EXISTS", "A match for the same game day already exists.");

        var match = Match.Create(
            _tenantContext.TenantId,
            cmd.GameDayId,
            homeTeamId,
            awayTeamId,
            cmd.Description);

        await _matchRepository.AddAsync(match, ct);
        await _matchRepository.SaveChangesAsync(ct);

        return Result<MatchResponse>.Ok(ToResponse(match));
    }

    private static MatchResponse ToResponse(Match match) => new(
        match.Id,
        match.TenantId,
        match.GameDayId,
        match.HomeTeamId,
        match.AwayTeamId,
        match.Description,
        match.Status,
        match.IsActive,
        match.CreatedAt);
}