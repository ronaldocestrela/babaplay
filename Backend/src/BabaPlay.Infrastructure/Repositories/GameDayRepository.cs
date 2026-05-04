using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class GameDayRepository : IGameDayRepository
{
    private readonly TenantDbContextFactory _factory;
    private readonly ITenantContext _tenantContext;

    public GameDayRepository(TenantDbContextFactory factory, ITenantContext tenantContext)
    {
        _factory = factory;
        _tenantContext = tenantContext;
    }

    public async Task<GameDay?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        return await db.GameDays.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id && g.IsActive, ct);
    }

    public async Task<IReadOnlyList<GameDay>> GetAllActiveAsync(GameDayStatus? status, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        var query = db.GameDays.AsNoTracking().Where(g => g.IsActive);

        if (status.HasValue)
            query = query.Where(g => g.Status == status.Value);

        return await query
            .OrderByDescending(g => g.ScheduledAt)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByNormalizedNameAndScheduledAtAsync(string normalizedName, DateTime scheduledAt, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        return await db.GameDays.AnyAsync(g => g.IsActive && g.NormalizedName == normalizedName && g.ScheduledAt == scheduledAt, ct);
    }

    public async Task AddAsync(GameDay gameDay, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        db.GameDays.Add(gameDay);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(GameDay gameDay, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        db.GameDays.Update(gameDay);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
