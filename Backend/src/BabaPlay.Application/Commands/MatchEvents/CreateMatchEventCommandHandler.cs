using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Commands.MatchEvents;

public sealed class CreateMatchEventCommandHandler
    : ICommandHandler<CreateMatchEventCommand, Result<MatchEventResponse>>
{
    private readonly IMatchEventRepository _eventRepository;
    private readonly IMatchEventTypeRepository _typeRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IMatchEventRealtimeNotifier _realtimeNotifier;

    public CreateMatchEventCommandHandler(
        IMatchEventRepository eventRepository,
        IMatchEventTypeRepository typeRepository,
        IMatchRepository matchRepository,
        ITeamRepository teamRepository,
        IPlayerRepository playerRepository,
        ITenantContext tenantContext,
        IMatchEventRealtimeNotifier realtimeNotifier)
    {
        _eventRepository = eventRepository;
        _typeRepository = typeRepository;
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _tenantContext = tenantContext;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<Result<MatchEventResponse>> HandleAsync(CreateMatchEventCommand cmd, CancellationToken ct = default)
    {
        if (cmd.MatchId == Guid.Empty)
            return Result<MatchEventResponse>.Fail("MATCH_EVENT_INVALID_MATCH_ID", "MatchId is required.");

        if (cmd.TeamId == Guid.Empty)
            return Result<MatchEventResponse>.Fail("MATCH_EVENT_INVALID_TEAM_ID", "TeamId is required.");

        if (cmd.PlayerId == Guid.Empty)
            return Result<MatchEventResponse>.Fail("MATCH_EVENT_INVALID_PLAYER_ID", "PlayerId is required.");

        if (cmd.MatchEventTypeId == Guid.Empty)
            return Result<MatchEventResponse>.Fail("MATCH_EVENT_INVALID_TYPE_ID", "MatchEventTypeId is required.");

        if (cmd.Minute < 0 || cmd.Minute > MatchEvent.MaxMinute)
            return Result<MatchEventResponse>.Fail("MATCH_EVENT_INVALID_MINUTE", $"Minute must be between 0 and {MatchEvent.MaxMinute}.");

        var match = await _matchRepository.GetByIdAsync(cmd.MatchId, ct);
        if (match is null)
            return Result<MatchEventResponse>.Fail("MATCH_EVENT_MATCH_NOT_FOUND", "Match was not found.");

        if (match.Status is MatchStatus.Cancelled or MatchStatus.Completed or MatchStatus.Pending)
            return Result<MatchEventResponse>.Fail("MATCH_EVENT_MATCH_NOT_OPEN", "Match is not open for event registration.");

        if (cmd.TeamId != match.HomeTeamId && cmd.TeamId != match.AwayTeamId)
            return Result<MatchEventResponse>.Fail("MATCH_EVENT_TEAM_NOT_IN_MATCH", "Team does not belong to this match.");

        var player = await _playerRepository.GetByIdAsync(cmd.PlayerId, ct);
        if (player is null || !player.IsActive)
            return Result<MatchEventResponse>.Fail("MATCH_EVENT_PLAYER_NOT_FOUND", "Player was not found.");

        var team = await _teamRepository.GetByIdAsync(cmd.TeamId, ct);
        if (team is null || !team.IsActive)
            return Result<MatchEventResponse>.Fail("MATCH_EVENT_TEAM_NOT_FOUND", "Team was not found.");

        if (!team.PlayerIds.Contains(cmd.PlayerId))
            return Result<MatchEventResponse>.Fail("MATCH_EVENT_PLAYER_NOT_IN_TEAM", "Player is not in the informed team roster.");

        var type = await _typeRepository.GetByIdAsync(cmd.MatchEventTypeId, ct);
        if (type is null)
            return Result<MatchEventResponse>.Fail("MATCH_EVENT_TYPE_NOT_FOUND", "Match event type was not found.");

        if (!type.IsActive)
            return Result<MatchEventResponse>.Fail("MATCH_EVENT_TYPE_INACTIVE", "Match event type is inactive.");

        var matchEvent = MatchEvent.Create(
            _tenantContext.TenantId,
            cmd.MatchId,
            cmd.TeamId,
            cmd.PlayerId,
            cmd.MatchEventTypeId,
            cmd.Minute,
            cmd.Notes);

        await _eventRepository.AddAsync(matchEvent, ct);
        await _eventRepository.SaveChangesAsync(ct);
        await _realtimeNotifier.NotifyMatchEventCreatedAsync(cmd.MatchId, matchEvent.Id, ct);

        return Result<MatchEventResponse>.Ok(ToResponse(matchEvent));
    }

    private static MatchEventResponse ToResponse(MatchEvent matchEvent) => new(
        matchEvent.Id,
        matchEvent.TenantId,
        matchEvent.MatchId,
        matchEvent.TeamId,
        matchEvent.PlayerId,
        matchEvent.MatchEventTypeId,
        matchEvent.Minute,
        matchEvent.Notes,
        matchEvent.IsActive,
        matchEvent.CreatedAt);
}
