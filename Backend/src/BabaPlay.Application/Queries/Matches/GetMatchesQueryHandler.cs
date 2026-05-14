using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Matches;

public sealed class GetMatchesQueryHandler
    : IQueryHandler<GetMatchesQuery, Result<IReadOnlyList<MatchResponse>>>
{
    private readonly IMatchRepository _matchRepository;

    public GetMatchesQueryHandler(IMatchRepository matchRepository)
        => _matchRepository = matchRepository;

    public async Task<Result<IReadOnlyList<MatchResponse>>> HandleAsync(GetMatchesQuery query, CancellationToken ct = default)
    {
        var matches = await _matchRepository.GetAllActiveAsync(query.Status, ct);

        return Result<IReadOnlyList<MatchResponse>>.Ok(matches
            .Select(match => new MatchResponse(
                match.Id,
                match.TenantId,
                match.GameDayId,
                match.HomeTeamId,
                match.AwayTeamId,
                match.Description,
                match.Status,
                match.IsActive,
                match.CreatedAt))
            .ToList());
    }
}