using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

public interface IMatchEventTypeRepository
{
    Task<MatchEventType?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<MatchEventType>> GetAllActiveAsync(CancellationToken ct = default);

    Task<bool> ExistsByNormalizedCodeAsync(string normalizedCode, Guid? excludeId, CancellationToken ct = default);

    Task AddAsync(MatchEventType matchEventType, CancellationToken ct = default);

    Task UpdateAsync(MatchEventType matchEventType, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
