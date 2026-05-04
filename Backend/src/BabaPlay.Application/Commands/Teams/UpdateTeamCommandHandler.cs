using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Teams;

public sealed class UpdateTeamCommandHandler : ICommandHandler<UpdateTeamCommand, Result<TeamResponse>>
{
    private readonly ITeamRepository _teamRepository;

    public UpdateTeamCommandHandler(ITeamRepository teamRepository)
        => _teamRepository = teamRepository;

    public async Task<Result<TeamResponse>> HandleAsync(UpdateTeamCommand cmd, CancellationToken ct = default)
    {
        var team = await _teamRepository.GetByIdAsync(cmd.TeamId, ct);
        if (team is null)
            return Result<TeamResponse>.Fail("TEAM_NOT_FOUND", $"Team '{cmd.TeamId}' was not found.");

        if (string.IsNullOrWhiteSpace(cmd.Name))
            return Result<TeamResponse>.Fail("INVALID_NAME", "Team name is required.");

        if (cmd.MaxPlayers <= 0)
            return Result<TeamResponse>.Fail("INVALID_MAX_PLAYERS", "MaxPlayers must be greater than zero.");

        var normalizedName = cmd.Name.Trim().ToUpperInvariant();
        if (!string.Equals(team.NormalizedName, normalizedName, StringComparison.Ordinal))
        {
            var exists = await _teamRepository.ExistsByNormalizedNameAsync(normalizedName, ct);
            if (exists)
                return Result<TeamResponse>.Fail("TEAM_ALREADY_EXISTS", $"Team '{normalizedName}' already exists.");
        }

        team.Update(cmd.Name, cmd.MaxPlayers);
        await _teamRepository.UpdateAsync(team, ct);
        await _teamRepository.SaveChangesAsync(ct);

        return Result<TeamResponse>.Ok(new TeamResponse(
            team.Id,
            team.TenantId,
            team.Name,
            team.MaxPlayers,
            team.IsActive,
            team.CreatedAt,
            team.PlayerIds.ToList()));
    }
}
