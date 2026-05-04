using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class CheckinRepository : ICheckinRepository
{
    private readonly TenantDbContextFactory _factory;
    private readonly ITenantContext _tenantContext;

    public CheckinRepository(TenantDbContextFactory factory, ITenantContext tenantContext)
    {
        _factory = factory;
        _tenantContext = tenantContext;
    }

    public async Task<bool> ExistsActiveByPlayerAndGameDayAsync(Guid playerId, Guid gameDayId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        return await db.Checkins.AnyAsync(
            c => c.IsActive && c.PlayerId == playerId && c.GameDayId == gameDayId,
            ct);
    }

    public async Task<int> CountActiveByGameDayAsync(Guid gameDayId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        return await db.Checkins.CountAsync(c => c.IsActive && c.GameDayId == gameDayId, ct);
    }

    public async Task AddAsync(Checkin checkin, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        db.Checkins.Add(checkin);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
