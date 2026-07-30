namespace BabaPlay.Application.DTOs;

/// <summary>
/// Response DTO for a single Announcement, including per-user read status.
/// </summary>
public sealed record AnnouncementResponse(
    Guid Id,
    Guid TenantId,
    Guid AuthorId,
    string AuthorName,
    string Title,
    string Content,
    bool IsPublished,
    DateTime? PublishedAtUtc,
    DateTime? ExpiresAtUtc,
    string? Tags,
    bool IsRead,
    bool IsNew,
    DateTime CreatedAt);
