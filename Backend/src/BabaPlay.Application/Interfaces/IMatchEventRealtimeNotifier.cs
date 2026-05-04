namespace BabaPlay.Application.Interfaces;

public interface IMatchEventRealtimeNotifier
{
    Task NotifyMatchEventCreatedAsync(Guid matchId, Guid matchEventId, CancellationToken ct = default);

    Task NotifyMatchEventUpdatedAsync(Guid matchId, Guid matchEventId, CancellationToken ct = default);

    Task NotifyMatchEventDeletedAsync(Guid matchId, Guid matchEventId, CancellationToken ct = default);
}
