using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

public interface IMatchEventRepository
{
    Task<MatchEvent?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<MatchEvent>> GetActiveByMatchAsync(Guid matchId, CancellationToken ct = default);

    Task<IReadOnlyList<MatchEvent>> GetActiveByPlayerAsync(Guid playerId, CancellationToken ct = default);

    Task AddAsync(MatchEvent matchEvent, CancellationToken ct = default);

    Task UpdateAsync(MatchEvent matchEvent, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
