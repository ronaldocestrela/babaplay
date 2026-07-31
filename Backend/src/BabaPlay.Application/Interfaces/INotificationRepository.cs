using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Repository contract for the Notification entity (F26 — Hub de Notificações).
/// </summary>
public interface INotificationRepository
{
    /// <summary>Returns notifications for a specific user, sorted by CreatedAt descending.</summary>
    Task<IReadOnlyList<Notification>> GetByUserAsync(
        Guid tenantId,
        Guid userId,
        bool onlyUnread = false,
        int limit = 20,
        CancellationToken ct = default);

    /// <summary>Returns a single notification by id and tenantId.</summary>
    Task<Notification?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);

    /// <summary>Returns the total number of unread notifications for a user.</summary>
    Task<int> GetUnreadCountAsync(Guid tenantId, Guid userId, CancellationToken ct = default);

    /// <summary>Tracks a new notification for persistence.</summary>
    Task AddAsync(Notification notification, CancellationToken ct = default);

    /// <summary>Tracks multiple notifications and persists them in a single transaction.</summary>
    Task AddRangeAsync(IReadOnlyList<Notification> notifications, CancellationToken ct = default);

    /// <summary>Updates an existing notification.</summary>
    Task UpdateAsync(Notification notification, CancellationToken ct = default);

    /// <summary>Marks all unread notifications as read for a user.</summary>
    Task MarkAllAsReadAsync(Guid tenantId, Guid userId, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
