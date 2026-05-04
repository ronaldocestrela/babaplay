using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Commands.MatchEvents;

public sealed class UpdateMatchEventCommandHandler
    : ICommandHandler<UpdateMatchEventCommand, Result<MatchEventResponse>>
{
    private readonly IMatchEventRepository _eventRepository;
    private readonly IMatchEventTypeRepository _typeRepository;
    private readonly IMatchEventRealtimeNotifier _realtimeNotifier;

    public UpdateMatchEventCommandHandler(
        IMatchEventRepository eventRepository,
        IMatchEventTypeRepository typeRepository,
        IMatchEventRealtimeNotifier realtimeNotifier)
    {
        _eventRepository = eventRepository;
        _typeRepository = typeRepository;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<Result<MatchEventResponse>> HandleAsync(UpdateMatchEventCommand cmd, CancellationToken ct = default)
    {
        var matchEvent = await _eventRepository.GetByIdAsync(cmd.MatchEventId, ct);
        if (matchEvent is null)
            return Result<MatchEventResponse>.Fail("MATCH_EVENT_NOT_FOUND", "Match event was not found.");

        var type = await _typeRepository.GetByIdAsync(cmd.MatchEventTypeId, ct);
        if (type is null)
            return Result<MatchEventResponse>.Fail("MATCH_EVENT_TYPE_NOT_FOUND", "Match event type was not found.");

        if (!type.IsActive)
            return Result<MatchEventResponse>.Fail("MATCH_EVENT_TYPE_INACTIVE", "Match event type is inactive.");

        if (cmd.Minute < 0 || cmd.Minute > MatchEvent.MaxMinute)
            return Result<MatchEventResponse>.Fail("MATCH_EVENT_INVALID_MINUTE", $"Minute must be between 0 and {MatchEvent.MaxMinute}.");

        matchEvent.Update(cmd.MatchEventTypeId, cmd.Minute, cmd.Notes);
        await _eventRepository.UpdateAsync(matchEvent, ct);
        await _eventRepository.SaveChangesAsync(ct);
        await _realtimeNotifier.NotifyMatchEventUpdatedAsync(matchEvent.MatchId, matchEvent.Id, ct);

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
