namespace BabaPlay.Application.Interfaces;

public interface ICheckinRealtimeNotifier
{
    Task NotifyCheckinCreatedAsync(Guid gameDayId, Guid playerId, CancellationToken ct = default);

    Task NotifyCheckinCountUpdatedAsync(Guid gameDayId, int activeCount, CancellationToken ct = default);

    Task NotifyCheckinDeniedAsync(Guid gameDayId, Guid playerId, string reasonCode, CancellationToken ct = default);

    Task NotifyCheckinUndoneAsync(Guid gameDayId, Guid playerId, CancellationToken ct = default);
}
