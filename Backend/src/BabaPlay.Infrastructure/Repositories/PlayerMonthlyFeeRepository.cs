using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class PlayerMonthlyFeeRepository : IPlayerMonthlyFeeRepository
{
    private readonly TenantDbContextFactory _factory;
    private readonly ITenantContext _tenantContext;

    public PlayerMonthlyFeeRepository(TenantDbContextFactory factory, ITenantContext tenantContext)
    {
        _factory = factory;
        _tenantContext = tenantContext;
    }

    public async Task<PlayerMonthlyFee?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        return await db.PlayerMonthlyFees.FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);
    }

    public async Task<IReadOnlyList<PlayerMonthlyFee>> GetOverdueAsync(DateTime referenceUtc, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);

        return await db.PlayerMonthlyFees
            .AsNoTracking()
            .Where(x => x.IsActive && x.Status != Domain.Enums.MonthlyFeeStatus.Paid && x.Status != Domain.Enums.MonthlyFeeStatus.Cancelled && x.DueDateUtc < referenceUtc)
            .OrderBy(x => x.DueDateUtc)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PlayerMonthlyFee>> GetByCompetenceAsync(int year, int month, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);

        return await db.PlayerMonthlyFees
            .AsNoTracking()
            .Where(x => x.IsActive && x.Year == year && x.Month == month)
            .OrderBy(x => x.PlayerId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PlayerMonthlyFee>> GetByPlayerAndPeriodAsync(
        Guid playerId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);

        return await db.PlayerMonthlyFees
            .AsNoTracking()
            .Where(x => x.IsActive && x.PlayerId == playerId && x.DueDateUtc >= fromUtc && x.DueDateUtc <= toUtc)
            .OrderBy(x => x.DueDateUtc)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByPlayerAndCompetenceAsync(
        Guid tenantId,
        Guid playerId,
        int year,
        int month,
        CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        return await db.PlayerMonthlyFees.AnyAsync(
            x => x.IsActive
                && x.TenantId == tenantId
                && x.PlayerId == playerId
                && x.Year == year
                && x.Month == month,
            ct);
    }

    public async Task AddAsync(PlayerMonthlyFee monthlyFee, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        db.PlayerMonthlyFees.Add(monthlyFee);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PlayerMonthlyFee monthlyFee, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        db.PlayerMonthlyFees.Update(monthlyFee);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
