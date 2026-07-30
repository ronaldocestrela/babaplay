using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Interfaces;

public interface IPlayerMonthlyFeeRepository
{
    Task<PlayerMonthlyFee?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<PlayerMonthlyFee>> GetOverdueAsync(DateTime referenceUtc, CancellationToken ct = default);

    Task<IReadOnlyList<PlayerMonthlyFee>> GetByCompetenceAsync(int year, int month, CancellationToken ct = default);

    Task<IReadOnlyList<PlayerMonthlyFee>> GetInvoicesAsync(
        int? year,
        int? month,
        Guid? playerId,
        MonthlyFeeStatus? status,
        CancellationToken ct = default);

    Task<IReadOnlyList<PlayerMonthlyFee>> GetByPlayerAndPeriodAsync(
        Guid playerId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct = default);

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
