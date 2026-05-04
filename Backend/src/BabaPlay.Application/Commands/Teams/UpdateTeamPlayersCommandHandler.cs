using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Teams;

public sealed class UpdateTeamPlayersCommandHandler
    : ICommandHandler<UpdateTeamPlayersCommand, Result<TeamPlayersResponse>>
{
    private const string GoalkeeperCode = "GOLEIRO";

    private readonly ITeamRepository _teamRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IPositionRepository _positionRepository;

    public UpdateTeamPlayersCommandHandler(
        ITeamRepository teamRepository,
        IPlayerRepository playerRepository,
        IPositionRepository positionRepository)
    {
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _positionRepository = positionRepository;
    }

    public async Task<Result<TeamPlayersResponse>> HandleAsync(UpdateTeamPlayersCommand cmd, CancellationToken ct = default)
    {
        var team = await _teamRepository.GetByIdAsync(cmd.TeamId, ct);
        if (team is null)
            return Result<TeamPlayersResponse>.Fail("TEAM_NOT_FOUND", $"Team '{cmd.TeamId}' was not found.");

        var inputIds = cmd.PlayerIds.ToList();

        if (inputIds.Any(id => id == Guid.Empty))
            return Result<TeamPlayersResponse>.Fail("TEAM_INVALID_PLAYER_ID", "PlayerIds cannot contain empty values.");

        if (inputIds.Distinct().Count() != inputIds.Count)
            return Result<TeamPlayersResponse>.Fail("TEAM_DUPLICATE_PLAYERS", "PlayerIds cannot contain duplicates.");

        if (inputIds.Count > team.MaxPlayers)
            return Result<TeamPlayersResponse>.Fail("TEAM_PLAYERS_LIMIT_EXCEEDED", "Roster size exceeds team max players.");

        var players = await _playerRepository.GetByIdsAsync(inputIds, ct);
        if (players.Count != inputIds.Count || players.Any(p => !p.IsActive))
            return Result<TeamPlayersResponse>.Fail("TEAM_PLAYER_NOT_FOUND", "One or more players were not found.");

        var hasGoalkeeper = await HasGoalkeeperAsync(players, ct);
        if (inputIds.Count > 0 && !hasGoalkeeper)
            return Result<TeamPlayersResponse>.Fail("TEAM_GOALKEEPER_REQUIRED", "At least one goalkeeper is required in the roster.");

        team.SetPlayers(inputIds, hasGoalkeeper);

        await _teamRepository.UpdateAsync(team, ct);
        await _teamRepository.SaveChangesAsync(ct);

        return Result<TeamPlayersResponse>.Ok(new TeamPlayersResponse(
            team.Id,
            team.PlayerIds.ToList(),
            team.UpdatedAt));
    }

    private async Task<bool> HasGoalkeeperAsync(IReadOnlyList<Domain.Entities.Player> players, CancellationToken ct)
    {
        var positionIds = players
            .SelectMany(p => p.PositionIds)
            .Distinct()
            .ToList();

        if (positionIds.Count == 0)
            return false;

        var positions = await _positionRepository.GetByIdsAsync(positionIds, ct);
        var goalkeeperPositionIds = positions
            .Where(p => p.IsActive && string.Equals(p.NormalizedCode, GoalkeeperCode, StringComparison.Ordinal))
            .Select(p => p.Id)
            .ToHashSet();

        if (goalkeeperPositionIds.Count == 0)
            return false;

        return players.Any(player => player.PositionIds.Any(positionId => goalkeeperPositionIds.Contains(positionId)));
    }
}