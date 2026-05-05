using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class CashTransactionRepository : ICashTransactionRepository
{
    private readonly TenantDbContextFactory _factory;
    private readonly ITenantContext _tenantContext;

    public CashTransactionRepository(TenantDbContextFactory factory, ITenantContext tenantContext)
    {
        _factory = factory;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(CashTransaction transaction, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        db.CashTransactions.Add(transaction);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<CashTransaction>> GetByPeriodAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);

        return await db.CashTransactions
            .AsNoTracking()
            .Where(x => x.IsActive && x.OccurredOnUtc >= fromUtc && x.OccurredOnUtc <= toUtc)
            .OrderBy(x => x.OccurredOnUtc)
            .ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
