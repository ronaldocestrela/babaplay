using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

    public async Task<MonthlyFeePayment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        return await db.MonthlyFeePayments.FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);
    }

    public async Task UpdateAsync(MonthlyFeePayment payment, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        db.MonthlyFeePayments.Update(payment);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
