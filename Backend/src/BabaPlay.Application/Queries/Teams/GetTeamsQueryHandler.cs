using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Teams;

public sealed class GetTeamsQueryHandler : IQueryHandler<GetTeamsQuery, Result<IReadOnlyList<TeamResponse>>>
{
    private readonly ITeamRepository _teamRepository;

    public GetTeamsQueryHandler(ITeamRepository teamRepository)
        => _teamRepository = teamRepository;

    public async Task<Result<IReadOnlyList<TeamResponse>>> HandleAsync(GetTeamsQuery query, CancellationToken ct = default)
    {
        var teams = await _teamRepository.GetAllActiveAsync(ct);

        return Result<IReadOnlyList<TeamResponse>>.Ok(
            teams.Select(team => new TeamResponse(
                team.Id,
                team.TenantId,
                team.Name,
                team.MaxPlayers,
                team.IsActive,
                team.CreatedAt,
                team.PlayerIds.ToList())).ToList());
    }
}
