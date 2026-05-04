using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

public interface ICheckinRepository
{
    Task<Checkin?> GetByIdAsync(Guid checkinId, CancellationToken ct = default);

    Task<IReadOnlyList<Checkin>> GetActiveByGameDayAsync(Guid gameDayId, CancellationToken ct = default);

    Task<IReadOnlyList<Checkin>> GetActiveByPlayerAsync(Guid playerId, CancellationToken ct = default);

    Task<bool> ExistsActiveByPlayerAndGameDayAsync(Guid playerId, Guid gameDayId, CancellationToken ct = default);

    Task<int> CountActiveByGameDayAsync(Guid gameDayId, CancellationToken ct = default);

    Task AddAsync(Checkin checkin, CancellationToken ct = default);

    Task UpdateAsync(Checkin checkin, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
