using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<Notification>> GetByUserAsync(
        Guid tenantId,
        Guid userId,
        bool onlyUnread = false,
        int limit = 20,
        CancellationToken ct = default)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.TenantId == tenantId && n.UserId == userId && n.IsActive);

        if (onlyUnread)
            query = query.Where(n => !n.IsRead);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<Notification?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
        => await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.TenantId == tenantId && n.IsActive, ct);

    public async Task<int> GetUnreadCountAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
        => await _context.Notifications
            .AsNoTracking()
            .CountAsync(n => n.TenantId == tenantId && n.UserId == userId && !n.IsRead && n.IsActive, ct);

    public async Task AddAsync(Notification notification, CancellationToken ct = default)
    {
        await _context.Notifications.AddAsync(notification, ct);
        await SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Notification notification, CancellationToken ct = default)
    {
        _context.Notifications.Update(notification);
        await SaveChangesAsync(ct);
    }

    public async Task MarkAllAsReadAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var unread = await _context.Notifications
            .Where(n => n.TenantId == tenantId && n.UserId == userId && !n.IsRead && n.IsActive)
            .ToListAsync(ct);

        foreach (var item in unread)
        {
            item.MarkAsRead(now);
        }

        await SaveChangesAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
