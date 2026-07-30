using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

/// <summary>
/// HTTP service interface for the Communication module (F24 — Announcements).
/// All calls include the Bearer token and X-Tenant-Id header via AuthorizationHeaderHandler.
/// </summary>
public interface ICommunicationApiService
{
    /// <summary>Returns all published announcements for the current tenant, with IsRead per user.</summary>
    Task<List<AnnouncementDto>?> GetAnnouncementsAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates and immediately publishes a new announcement. Requires CommunicationWrite permission.</summary>
    Task<AnnouncementDto?> CreateAnnouncementAsync(CreateAnnouncementDto dto, CancellationToken cancellationToken = default);

    /// <summary>Marks the given announcement as read by the current user. Idempotent.</summary>
    Task<bool> MarkAnnouncementReadAsync(Guid announcementId, CancellationToken cancellationToken = default);
}
