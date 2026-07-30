using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class PollRepository : IPollRepository
{
    private readonly AppDbContext _context;

    public PollRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<Poll>> GetByTenantAsync(
        Guid tenantId,
        bool onlyActive = true,
        CancellationToken ct = default)
    {
        var query = _context.Polls
            .AsNoTracking()
            .Include(p => p.Options)
            .Include(p => p.Votes)
            .Where(p => p.TenantId == tenantId && p.IsActive);

        if (onlyActive)
            query = query.Where(p => !p.IsClosed);

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Poll?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
        => await _context.Polls
            .Include(p => p.Options)
            .Include(p => p.Votes)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId && p.IsActive, ct);

    public async Task AddAsync(Poll poll, CancellationToken ct = default)
    {
        await _context.Polls.AddAsync(poll, ct);
        await SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Poll poll, CancellationToken ct = default)
    {
        _context.Polls.Update(poll);
        await SaveChangesAsync(ct);
    }

    public async Task AddVoteAsync(PollVote vote, CancellationToken ct = default)
    {
        await _context.PollVotes.AddAsync(vote, ct);
        await SaveChangesAsync(ct);
    }

    public async Task<bool> HasUserVotedAsync(Guid pollId, string userId, CancellationToken ct = default)
        => await _context.PollVotes
            .AsNoTracking()
            .AnyAsync(v => v.PollId == pollId && v.UserId == userId, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
