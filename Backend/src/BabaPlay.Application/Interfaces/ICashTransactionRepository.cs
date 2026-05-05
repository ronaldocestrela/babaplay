using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

public interface ICashTransactionRepository
{
    Task AddAsync(CashTransaction transaction, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
