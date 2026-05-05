using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

public interface IMonthlyFeePaymentRepository
{
    Task AddAsync(MonthlyFeePayment payment, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
