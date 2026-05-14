using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Interfaces;

public interface IGameDayRepository
{
    Task<GameDay?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<GameDay>> GetAllActiveAsync(GameDayStatus? status, CancellationToken ct = default);

    Task<bool> ExistsByNormalizedNameAndScheduledAtAsync(string normalizedName, DateTime scheduledAt, CancellationToken ct = default);

    Task AddAsync(GameDay gameDay, CancellationToken ct = default);

    Task UpdateAsync(GameDay gameDay, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
