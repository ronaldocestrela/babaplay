using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class MatchRepository : IMatchRepository
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public MatchRepository(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Match?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var db = _db;
        return await db.Matches.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id && m.IsActive, ct);
    }

    public async Task<IReadOnlyList<Match>> GetAllActiveAsync(MatchStatus? status, CancellationToken ct = default)
    {
        var db = _db;
        var query = db.Matches.AsNoTracking().Where(m => m.IsActive);

        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByGameDayAsync(Guid gameDayId, Guid? excludeMatchId, CancellationToken ct = default)
    {
        var db = _db;

        var query = db.Matches.Where(m => m.IsActive && m.GameDayId == gameDayId);

        if (excludeMatchId.HasValue)
            query = query.Where(m => m.Id != excludeMatchId.Value);

        return await query.AnyAsync(ct);
    }

    public async Task<bool> ExistsByGameDayAndTeamsAsync(
        Guid gameDayId,
        Guid homeTeamId,
        Guid awayTeamId,
        Guid? excludeMatchId,
        CancellationToken ct = default)
    {
        var db = _db;

        var query = db.Matches.Where(m => m.IsActive && m.GameDayId == gameDayId);

        if (excludeMatchId.HasValue)
            query = query.Where(m => m.Id != excludeMatchId.Value);

        return await query.AnyAsync(m =>
            (m.HomeTeamId == homeTeamId && m.AwayTeamId == awayTeamId)
            || (m.HomeTeamId == awayTeamId && m.AwayTeamId == homeTeamId),
            ct);
    }

    public async Task AddAsync(Match match, CancellationToken ct = default)
    {
        var db = _db;
        db.Matches.Add(match);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Match match, CancellationToken ct = default)
    {
        var db = _db;
        db.Matches.Update(match);
        await db.SaveChangesAsync(ct);
    }

    // No-op because AddAsync/UpdateAsync create isolated contexts and persist immediately.
    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}