using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class MonthlyFeePaymentRepository : IMonthlyFeePaymentRepository
{
    private readonly TenantDbContextFactory _factory;
    private readonly ITenantContext _tenantContext;

    public MonthlyFeePaymentRepository(TenantDbContextFactory factory, ITenantContext tenantContext)
    {
        _factory = factory;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(MonthlyFeePayment payment, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        db.MonthlyFeePayments.Add(payment);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
