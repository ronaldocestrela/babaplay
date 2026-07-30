using System.Collections.Generic;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Announcements;

/// <summary>
/// Returns all published announcements for the current tenant,
/// annotated with the read status of the requesting user.
/// </summary>
public sealed record GetAnnouncementsQuery(
    bool OnlyPublished = true,
    string? RequestingUserId = null) : IQuery<Result<IReadOnlyList<AnnouncementResponse>>>;
