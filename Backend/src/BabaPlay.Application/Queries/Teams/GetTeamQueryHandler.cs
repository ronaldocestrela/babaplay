using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Teams;

public sealed class GetTeamQueryHandler : IQueryHandler<GetTeamQuery, Result<TeamResponse>>
{
    private readonly ITeamRepository _teamRepository;

    public GetTeamQueryHandler(ITeamRepository teamRepository)
        => _teamRepository = teamRepository;

    public async Task<Result<TeamResponse>> HandleAsync(GetTeamQuery query, CancellationToken ct = default)
    {
        var team = await _teamRepository.GetByIdAsync(query.TeamId, ct);
        if (team is null || !team.IsActive)
            return Result<TeamResponse>.Fail("TEAM_NOT_FOUND", $"Team '{query.TeamId}' was not found.");

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
