using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Repository contract for the Announcement aggregate (F24 — Communication module).
/// </summary>
public interface IAnnouncementRepository
{
    /// <summary>Returns all published (and optionally active) announcements for a tenant, newest first.</summary>
    Task<IReadOnlyList<Announcement>> GetByTenantAsync(
        Guid tenantId,
        bool onlyPublished = true,
        CancellationToken ct = default);

    /// <summary>Returns a single announcement by id, validated against the tenant.</summary>
    Task<Announcement?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);

    /// <summary>Persists a new announcement and saves immediately.</summary>
    Task AddAsync(Announcement announcement, CancellationToken ct = default);

    /// <summary>Records that a user has read an announcement.</summary>
    Task AddReadAsync(AnnouncementRead read, CancellationToken ct = default);

    /// <summary>Returns true if the given user has already read the given announcement.</summary>
    Task<bool> HasUserReadAsync(Guid announcementId, string userId, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
