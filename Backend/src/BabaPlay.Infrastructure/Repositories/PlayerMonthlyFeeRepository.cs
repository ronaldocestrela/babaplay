using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class PlayerMonthlyFeeRepository : IPlayerMonthlyFeeRepository
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public PlayerMonthlyFeeRepository(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<PlayerMonthlyFee?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var db = _db;
        return await db.PlayerMonthlyFees.FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);
    }

    public async Task<IReadOnlyList<PlayerMonthlyFee>> GetOverdueAsync(DateTime referenceUtc, CancellationToken ct = default)
    {
        var db = _db;

        return await db.PlayerMonthlyFees
            .AsNoTracking()
            .Where(x => x.IsActive && x.Status != Domain.Enums.MonthlyFeeStatus.Paid && x.Status != Domain.Enums.MonthlyFeeStatus.Cancelled && x.DueDateUtc < referenceUtc)
            .OrderBy(x => x.DueDateUtc)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PlayerMonthlyFee>> GetByCompetenceAsync(int year, int month, CancellationToken ct = default)
    {
        var db = _db;

        return await db.PlayerMonthlyFees
            .AsNoTracking()
            .Where(x => x.IsActive && x.Year == year && x.Month == month)
            .OrderBy(x => x.PlayerId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PlayerMonthlyFee>> GetInvoicesAsync(
        int? year,
        int? month,
        Guid? playerId,
        BabaPlay.Domain.Enums.MonthlyFeeStatus? status,
        CancellationToken ct = default)
    {
        var db = _db;
        var query = db.PlayerMonthlyFees.AsNoTracking().Where(x => x.IsActive);

        if (year.HasValue && year.Value > 0)
            query = query.Where(x => x.Year == year.Value);

        if (month.HasValue && month.Value > 0)
            query = query.Where(x => x.Month == month.Value);

        if (playerId.HasValue && playerId.Value != Guid.Empty)
            query = query.Where(x => x.PlayerId == playerId.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        return await query
            .OrderByDescending(x => x.DueDateUtc)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PlayerMonthlyFee>> GetByPlayerAndPeriodAsync(
        Guid playerId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct = default)
    {
        var db = _db;

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
        var db = _db;
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
        var db = _db;
        db.PlayerMonthlyFees.Add(monthlyFee);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PlayerMonthlyFee monthlyFee, CancellationToken ct = default)
    {
        var db = _db;
        db.PlayerMonthlyFees.Update(monthlyFee);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
