using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

public interface ICheckinRepository
{
    Task<bool> ExistsActiveByPlayerAndGameDayAsync(Guid playerId, Guid gameDayId, CancellationToken ct = default);

    Task<int> CountActiveByGameDayAsync(Guid gameDayId, CancellationToken ct = default);

    Task AddAsync(Checkin checkin, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
