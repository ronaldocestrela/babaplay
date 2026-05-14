using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class MatchSummaryRepository : IMatchSummaryRepository
{
    private readonly TenantDbContextFactory _factory;
    private readonly ITenantContext _tenantContext;

    public MatchSummaryRepository(TenantDbContextFactory factory, ITenantContext tenantContext)
    {
        _factory = factory;
        _tenantContext = tenantContext;
    }

    public async Task<MatchSummary?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        return await db.MatchSummaries.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);
    }

    public async Task<MatchSummary?> GetByMatchIdAsync(Guid matchId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        return await db.MatchSummaries.AsNoTracking().FirstOrDefaultAsync(x => x.MatchId == matchId && x.IsActive, ct);
    }

    public async Task AddAsync(MatchSummary summary, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        db.MatchSummaries.Add(summary);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(MatchSummary summary, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        db.MatchSummaries.Update(summary);
        await db.SaveChangesAsync(ct);
    }

    // No-op because AddAsync/UpdateAsync create isolated contexts and persist immediately.
    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
