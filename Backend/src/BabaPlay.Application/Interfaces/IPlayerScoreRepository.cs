using BabaPlay.Domain.Entities;
using BabaPlay.Domain.ValueObjects;

namespace BabaPlay.Application.Interfaces;

public interface IPlayerScoreRepository
{
    Task<PlayerScore?> GetByPlayerIdAsync(Guid playerId, CancellationToken ct = default);

    Task<IReadOnlyList<PlayerScore>> GetRankingAsync(RankingPeriod? period, int skip, int take, CancellationToken ct = default);

    Task<IReadOnlyList<PlayerScore>> GetTopScorersAsync(RankingPeriod? period, int skip, int take, CancellationToken ct = default);

    Task<IReadOnlyList<PlayerScore>> GetAttendanceRankingAsync(RankingPeriod? period, int skip, int take, CancellationToken ct = default);

    Task<bool> HasProcessedSourceEventAsync(Guid sourceEventId, CancellationToken ct = default);

    Task AddProcessedSourceEventAsync(PlayerScoreSourceEvent sourceEvent, CancellationToken ct = default);

    Task AddAsync(PlayerScore playerScore, CancellationToken ct = default);

    Task UpdateAsync(PlayerScore playerScore, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}