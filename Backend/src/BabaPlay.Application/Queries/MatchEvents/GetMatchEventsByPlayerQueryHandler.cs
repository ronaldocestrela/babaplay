using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.MatchEvents;

public sealed class GetMatchEventsByPlayerQueryHandler
    : IQueryHandler<GetMatchEventsByPlayerQuery, Result<IReadOnlyList<MatchEventResponse>>>
{
    private readonly IMatchEventRepository _eventRepository;

    public GetMatchEventsByPlayerQueryHandler(IMatchEventRepository eventRepository)
        => _eventRepository = eventRepository;

    public async Task<Result<IReadOnlyList<MatchEventResponse>>> HandleAsync(GetMatchEventsByPlayerQuery query, CancellationToken ct = default)
    {
        var items = await _eventRepository.GetActiveByPlayerAsync(query.PlayerId, ct);

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
