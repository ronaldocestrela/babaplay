using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class AnnouncementRepository : IAnnouncementRepository
{
    private readonly AppDbContext _context;

    public AnnouncementRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<Announcement>> GetByTenantAsync(
        Guid tenantId,
        bool onlyPublished = true,
        CancellationToken ct = default)
    {
        var query = _context.Announcements
            .AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.IsActive);

        if (onlyPublished)
            query = query.Where(a => a.IsPublished);

        return await query
            .OrderByDescending(a => a.PublishedAtUtc ?? a.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Announcement?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
        => await _context.Announcements
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId && a.IsActive, ct);

    public async Task AddAsync(Announcement announcement, CancellationToken ct = default)
    {
        await _context.Announcements.AddAsync(announcement, ct);
        await SaveChangesAsync(ct);
    }

    public async Task AddReadAsync(AnnouncementRead read, CancellationToken ct = default)
    {
        await _context.AnnouncementReads.AddAsync(read, ct);
        await SaveChangesAsync(ct);
    }

    public async Task<bool> HasUserReadAsync(Guid announcementId, string userId, CancellationToken ct = default)
        => await _context.AnnouncementReads
            .AsNoTracking()
            .AnyAsync(r => r.AnnouncementId == announcementId && r.UserId == userId, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
