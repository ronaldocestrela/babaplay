using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.MatchEvents;

public sealed class GetMatchEventsByMatchQueryHandler
    : IQueryHandler<GetMatchEventsByMatchQuery, Result<IReadOnlyList<MatchEventResponse>>>
{
    private readonly IMatchEventRepository _eventRepository;

    public GetMatchEventsByMatchQueryHandler(IMatchEventRepository eventRepository)
        => _eventRepository = eventRepository;

    public async Task<Result<IReadOnlyList<MatchEventResponse>>> HandleAsync(GetMatchEventsByMatchQuery query, CancellationToken ct = default)
    {
        var items = await _eventRepository.GetActiveByMatchAsync(query.MatchId, ct);

        return Result<IReadOnlyList<MatchEventResponse>>.Ok(items
            .Select(x => new MatchEventResponse(
                x.Id,
                x.TenantId,
                x.MatchId,
                x.TeamId,
                x.PlayerId,
                x.MatchEventTypeId,
                x.Minute,
                x.Notes,
                x.IsActive,
                x.CreatedAt))
            .ToList());
    }
}
