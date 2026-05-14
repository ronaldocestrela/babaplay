using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Persistence abstraction for tenant-scoped positions.
/// </summary>
public interface IPositionRepository
{
    Task<Position?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Position>> GetAllActiveAsync(CancellationToken ct = default);

    Task<bool> ExistsByNormalizedCodeAsync(string normalizedCode, CancellationToken ct = default);

    Task<bool> IsInUseAsync(Guid positionId, CancellationToken ct = default);

    Task<IReadOnlyList<Position>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);

    Task AddAsync(Position position, CancellationToken ct = default);

    Task UpdateAsync(Position position, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
