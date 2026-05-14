using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BabaPlay.Infrastructure.Services;

public sealed class SignalRCheckinRealtimeNotifier : ICheckinRealtimeNotifier
{
    private readonly IHubContext<CheckinHub> _hubContext;

    public SignalRCheckinRealtimeNotifier(IHubContext<CheckinHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyCheckinCreatedAsync(Guid gameDayId, Guid playerId, CancellationToken ct = default)
        => _hubContext.Clients
            .Group(CheckinHub.GameDayGroup(gameDayId))
            .SendAsync(CheckinHub.CheckinCreatedEvent, new { gameDayId, playerId }, ct);

    public Task NotifyCheckinCountUpdatedAsync(Guid gameDayId, int activeCount, CancellationToken ct = default)
        => _hubContext.Clients
            .Group(CheckinHub.GameDayGroup(gameDayId))
            .SendAsync(CheckinHub.CheckinCountUpdatedEvent, new { gameDayId, activeCount }, ct);

    public Task NotifyCheckinDeniedAsync(Guid gameDayId, Guid playerId, string reasonCode, CancellationToken ct = default)
        => _hubContext.Clients
            .Group(CheckinHub.GameDayGroup(gameDayId))
            .SendAsync(CheckinHub.CheckinDeniedEvent, new { gameDayId, playerId, reasonCode }, ct);

    public Task NotifyCheckinUndoneAsync(Guid gameDayId, Guid playerId, CancellationToken ct = default)
        => _hubContext.Clients
            .Group(CheckinHub.GameDayGroup(gameDayId))
            .SendAsync(CheckinHub.CheckinUndoneEvent, new { gameDayId, playerId }, ct);
}
