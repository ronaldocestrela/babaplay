using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

public interface IPlayerMonthlyFeeRepository
{
    Task<PlayerMonthlyFee?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByPlayerAndCompetenceAsync(
        Guid tenantId,
        Guid playerId,
        int year,
        int month,
        CancellationToken ct = default);

    Task AddAsync(PlayerMonthlyFee monthlyFee, CancellationToken ct = default);

    Task UpdateAsync(PlayerMonthlyFee monthlyFee, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
