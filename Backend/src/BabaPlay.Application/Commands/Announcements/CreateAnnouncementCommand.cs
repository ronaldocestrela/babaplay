using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Announcements;

/// <summary>
/// Creates and publishes a new announcement for the current tenant.
/// Only users with Admin or board roles should dispatch this command.
/// </summary>
public sealed record CreateAnnouncementCommand(
    Guid AuthorId,
    string Title,
    string Content,
    DateTime? ExpiresAtUtc = null,
    string? Tags = null) : ICommand<Result<AnnouncementResponse>>;
