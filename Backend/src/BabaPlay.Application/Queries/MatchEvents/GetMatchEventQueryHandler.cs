using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.MatchEvents;

public sealed class GetMatchEventQueryHandler
    : IQueryHandler<GetMatchEventQuery, Result<MatchEventResponse>>
{
    private readonly IMatchEventRepository _eventRepository;

    public GetMatchEventQueryHandler(IMatchEventRepository eventRepository)
        => _eventRepository = eventRepository;

    public async Task<Result<MatchEventResponse>> HandleAsync(GetMatchEventQuery query, CancellationToken ct = default)
    {
        var matchEvent = await _eventRepository.GetByIdAsync(query.MatchEventId, ct);
        if (matchEvent is null)
            return Result<MatchEventResponse>.Fail("MATCH_EVENT_NOT_FOUND", "Match event was not found.");

        return Result<MatchEventResponse>.Ok(new MatchEventResponse(
            matchEvent.Id,
            matchEvent.TenantId,
            matchEvent.MatchId,
            matchEvent.TeamId,
            matchEvent.PlayerId,
            matchEvent.MatchEventTypeId,
            matchEvent.Minute,
            matchEvent.Notes,
            matchEvent.IsActive,
            matchEvent.CreatedAt));
    }
}
