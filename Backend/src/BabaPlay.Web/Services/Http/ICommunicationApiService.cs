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

    // ─── Enquetes (F25) ───────────────────────────────────────────────────────

    /// <summary>Returns all polls for the current tenant with option breakdowns and vote status.</summary>
    Task<List<PollDto>?> GetPollsAsync(bool onlyActive = true, CancellationToken cancellationToken = default);

    /// <summary>Creates a new interactive poll with choices. Requires CommunicationWrite permission.</summary>
    Task<PollDto?> CreatePollAsync(CreatePollDto dto, CancellationToken cancellationToken = default);

    /// <summary>Submits a vote for a poll option.</summary>
    Task<PollDto?> SubmitPollVoteAsync(Guid pollId, SubmitPollVoteDto dto, CancellationToken cancellationToken = default);

    /// <summary>Manually closes a poll.</summary>
    Task<bool> ClosePollAsync(Guid pollId, CancellationToken cancellationToken = default);
}
