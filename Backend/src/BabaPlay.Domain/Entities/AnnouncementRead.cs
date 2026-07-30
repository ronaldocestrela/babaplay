namespace BabaPlay.Domain.Entities;

/// <summary>
/// Junction entity that records which user read which announcement and when.
/// Enables per-user read tracking without modifying the Announcement aggregate.
/// </summary>
public sealed class AnnouncementRead
{
    public Guid AnnouncementId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public DateTime ReadAtUtc { get; private set; }

    // Navigation
    public Announcement Announcement { get; private set; } = null!;

    private AnnouncementRead() { }

    public static AnnouncementRead Create(Guid announcementId, string userId)
    {
        if (announcementId == Guid.Empty)
            throw new ArgumentException("AnnouncementId is required.", nameof(announcementId));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));

        return new AnnouncementRead
        {
            AnnouncementId = announcementId,
            UserId = userId,
            ReadAtUtc = DateTime.UtcNow,
        };
    }
}
