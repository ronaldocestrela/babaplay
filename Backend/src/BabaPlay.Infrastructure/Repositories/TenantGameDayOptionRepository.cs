using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class TenantGameDayOptionRepository : ITenantGameDayOptionRepository
{
    private readonly MasterDbContext _context;

    public TenantGameDayOptionRepository(MasterDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TenantGameDayOption>> GetByTenantAsync(Guid tenantId, bool? isActive, CancellationToken ct = default)
    {
        var query = _context.Set<TenantGameDayOption>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        return await query
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.LocalStartTime)
            .ToListAsync(ct);
    }

    public async Task<TenantGameDayOption?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<TenantGameDayOption>().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<bool> ExistsActiveBySlotAsync(Guid tenantId, DayOfWeek dayOfWeek, TimeOnly localStartTime, Guid? excludingId = null, CancellationToken ct = default)
    {
        var query = _context.Set<TenantGameDayOption>()
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId &&
                x.IsActive &&
                x.DayOfWeek == dayOfWeek &&
                x.LocalStartTime == localStartTime);

        if (excludingId.HasValue)
            query = query.Where(x => x.Id != excludingId.Value);

        return await query.AnyAsync(ct);
    }

    public async Task AddAsync(TenantGameDayOption option, CancellationToken ct = default)
    {
        _context.Set<TenantGameDayOption>().Add(option);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TenantGameDayOption option, CancellationToken ct = default)
    {
        _context.Set<TenantGameDayOption>().Update(option);
        await _context.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
