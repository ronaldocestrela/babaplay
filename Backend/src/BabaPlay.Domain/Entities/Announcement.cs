namespace BabaPlay.Domain.Entities;

/// <summary>
/// Represents an official announcement published by the association's board to all members.
/// Announcements are tenant-scoped and support an optional expiry date and tagging.
/// </summary>
public sealed class Announcement : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public bool IsPublished { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public string? Tags { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation — EF Core only
    public ICollection<AnnouncementRead> Reads { get; private set; } = [];

    private Announcement() { }

    /// <summary>
    /// Creates a new <see cref="Announcement"/> in draft state.
    /// Call <see cref="Publish"/> to make it visible to members.
    /// </summary>
    public static Announcement Create(
        Guid tenantId,
        Guid authorId,
        string title,
        string content,
        DateTime? expiresAtUtc = null,
        string? tags = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));

        if (authorId == Guid.Empty)
            throw new ArgumentException("AuthorId is required.", nameof(authorId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required.", nameof(content));

        return new Announcement
        {
            TenantId = tenantId,
            AuthorId = authorId,
            Title = title.Trim(),
            Content = content.Trim(),
            IsPublished = false,
            PublishedAtUtc = null,
            ExpiresAtUtc = expiresAtUtc?.ToUniversalTime(),
            Tags = string.IsNullOrWhiteSpace(tags) ? null : tags.Trim(),
            IsActive = true,
        };
    }

    /// <summary>
    /// Publishes the announcement, making it visible to all tenant members.
    /// Idempotent — calling multiple times is safe.
    /// </summary>
    public void Publish()
    {
        if (IsPublished)
            return;

        IsPublished = true;
        PublishedAtUtc = DateTime.UtcNow;
        MarkUpdated();
    }

    /// <summary>
    /// Unpublishes the announcement (hides from members without deleting it).
    /// </summary>
    public void Unpublish()
    {
        if (!IsPublished)
            return;

        IsPublished = false;
        MarkUpdated();
    }

    /// <summary>Sets or clears the expiry date for the announcement.</summary>
    public void SetExpiry(DateTime? expiresAtUtc)
    {
        ExpiresAtUtc = expiresAtUtc?.ToUniversalTime();
        MarkUpdated();
    }

    /// <summary>Soft-deletes the announcement.</summary>
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        IsPublished = false;
        MarkUpdated();
    }
}
