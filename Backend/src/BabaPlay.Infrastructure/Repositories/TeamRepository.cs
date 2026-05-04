using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

/// <summary>
/// Team repository backed by the per-tenant isolated database.
/// </summary>
public sealed class TeamRepository : ITeamRepository
{
    private readonly TenantDbContextFactory _factory;
    private readonly ITenantContext _tenantContext;

    public TeamRepository(TenantDbContextFactory factory, ITenantContext tenantContext)
    {
        _factory = factory;
        _tenantContext = tenantContext;
    }

    public async Task<Team?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        return await db.Teams
            .Include(t => t.Players)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<IReadOnlyList<Team>> GetAllActiveAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        return await db.Teams
            .Include(t => t.Players)
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        return await db.Teams.AnyAsync(t => t.NormalizedName == normalizedName, ct);
    }

    public async Task AddAsync(Team team, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);
        db.Teams.Add(team);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Team team, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateAsync(_tenantContext.TenantId, ct);

        var existing = await db.Teams
            .Include(t => t.Players)
            .FirstOrDefaultAsync(t => t.Id == team.Id, ct);

        if (existing is null)
        {
            db.Teams.Update(team);
            await db.SaveChangesAsync(ct);
            return;
        }

        db.Entry(existing).CurrentValues.SetValues(team);

        db.TeamPlayers.RemoveRange(existing.Players);
        foreach (var playerId in team.PlayerIds)
            db.TeamPlayers.Add(TeamPlayer.Create(existing.Id, playerId));

        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}