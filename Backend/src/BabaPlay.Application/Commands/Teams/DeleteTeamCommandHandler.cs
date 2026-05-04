using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Teams;

public sealed class DeleteTeamCommandHandler : ICommandHandler<DeleteTeamCommand, Result>
{
    private readonly ITeamRepository _teamRepository;

    public DeleteTeamCommandHandler(ITeamRepository teamRepository)
        => _teamRepository = teamRepository;

    public async Task<Result> HandleAsync(DeleteTeamCommand cmd, CancellationToken ct = default)
    {
        var team = await _teamRepository.GetByIdAsync(cmd.TeamId, ct);
        if (team is null)
            return Result.Fail("TEAM_NOT_FOUND", $"Team '{cmd.TeamId}' was not found.");

        team.Deactivate();
        await _teamRepository.UpdateAsync(team, ct);
        await _teamRepository.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
