using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

public interface ICashTransactionRepository
{
    Task<IReadOnlyList<CashTransaction>> GetByPeriodAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);

    Task AddAsync(CashTransaction transaction, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
