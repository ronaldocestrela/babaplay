using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Matches;

public sealed class GetMatchQueryHandler
    : IQueryHandler<GetMatchQuery, Result<MatchResponse>>
{
    private readonly IMatchRepository _matchRepository;

    public GetMatchQueryHandler(IMatchRepository matchRepository)
        => _matchRepository = matchRepository;

    public async Task<Result<MatchResponse>> HandleAsync(GetMatchQuery query, CancellationToken ct = default)
    {
        var match = await _matchRepository.GetByIdAsync(query.MatchId, ct);
        if (match is null)
            return Result<MatchResponse>.Fail("MATCH_NOT_FOUND", $"Match '{query.MatchId}' was not found.");

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