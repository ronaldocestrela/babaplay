using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

public interface IMatchSummaryRepository
{
    Task<MatchSummary?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<MatchSummary?> GetByMatchIdAsync(Guid matchId, CancellationToken ct = default);

    Task AddAsync(MatchSummary summary, CancellationToken ct = default);

    Task UpdateAsync(MatchSummary summary, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
