using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Announcements;

/// <summary>
/// Records that a specific user has read an announcement.
/// Idempotent: dispatching multiple times for the same user/announcement is safe.
/// </summary>
public sealed record MarkAnnouncementReadCommand(
    Guid AnnouncementId,
    string UserId) : ICommand<Result>;
