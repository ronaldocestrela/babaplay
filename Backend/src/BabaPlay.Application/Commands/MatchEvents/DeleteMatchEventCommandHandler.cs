using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.MatchEvents;

public sealed class DeleteMatchEventCommandHandler
    : ICommandHandler<DeleteMatchEventCommand, Result>
{
    private readonly IMatchEventRepository _eventRepository;
    private readonly IMatchEventRealtimeNotifier _realtimeNotifier;

    public DeleteMatchEventCommandHandler(
        IMatchEventRepository eventRepository,
        IMatchEventRealtimeNotifier realtimeNotifier)
    {
        _eventRepository = eventRepository;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<Result> HandleAsync(DeleteMatchEventCommand cmd, CancellationToken ct = default)
    {
        var matchEvent = await _eventRepository.GetByIdAsync(cmd.MatchEventId, ct);
        if (matchEvent is null)
            return Result.Fail("MATCH_EVENT_NOT_FOUND", "Match event was not found.");

        matchEvent.Deactivate();
        await _eventRepository.UpdateAsync(matchEvent, ct);
        await _eventRepository.SaveChangesAsync(ct);
        await _realtimeNotifier.NotifyMatchEventDeletedAsync(matchEvent.MatchId, matchEvent.Id, ct);

        return Result.Ok();
    }
}
