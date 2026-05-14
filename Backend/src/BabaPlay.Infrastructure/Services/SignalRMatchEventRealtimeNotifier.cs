using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BabaPlay.Infrastructure.Services;

public sealed class SignalRMatchEventRealtimeNotifier : IMatchEventRealtimeNotifier
{
    private readonly IHubContext<MatchHub> _hubContext;

    public SignalRMatchEventRealtimeNotifier(IHubContext<MatchHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyMatchEventCreatedAsync(Guid matchId, Guid matchEventId, CancellationToken ct = default)
        => _hubContext.Clients
            .Group(MatchHub.MatchGroup(matchId))
            .SendAsync(MatchHub.MatchEventCreatedEvent, new { matchId, matchEventId }, ct);

    public Task NotifyMatchEventUpdatedAsync(Guid matchId, Guid matchEventId, CancellationToken ct = default)
        => _hubContext.Clients
            .Group(MatchHub.MatchGroup(matchId))
            .SendAsync(MatchHub.MatchEventUpdatedEvent, new { matchId, matchEventId }, ct);

    public Task NotifyMatchEventDeletedAsync(Guid matchId, Guid matchEventId, CancellationToken ct = default)
        => _hubContext.Clients
            .Group(MatchHub.MatchGroup(matchId))
            .SendAsync(MatchHub.MatchEventDeletedEvent, new { matchId, matchEventId }, ct);
}
