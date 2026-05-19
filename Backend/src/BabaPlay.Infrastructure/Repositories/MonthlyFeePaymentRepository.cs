using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class MonthlyFeePaymentRepository : IMonthlyFeePaymentRepository
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public MonthlyFeePaymentRepository(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(MonthlyFeePayment payment, CancellationToken ct = default)
    {
        var db = _db;
        db.MonthlyFeePayments.Add(payment);
        await db.SaveChangesAsync(ct);
    }

    public async Task<MonthlyFeePayment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var db = _db;
        return await db.MonthlyFeePayments.FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);
    }

    public async Task UpdateAsync(MonthlyFeePayment payment, CancellationToken ct = default)
    {
        var db = _db;
        db.MonthlyFeePayments.Update(payment);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
