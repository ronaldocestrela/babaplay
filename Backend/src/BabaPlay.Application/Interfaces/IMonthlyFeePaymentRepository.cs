using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

public interface IMonthlyFeePaymentRepository
{
    Task<MonthlyFeePayment?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(MonthlyFeePayment payment, CancellationToken ct = default);

    Task UpdateAsync(MonthlyFeePayment payment, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
