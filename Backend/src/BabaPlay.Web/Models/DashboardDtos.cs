using System;
using System.Collections.Generic;

namespace BabaPlay.Web.Models;

public record NextMatchWidgetDto(
    Guid MatchId,
    string Title,
    DateTime Date,
    string Location,
    int ConfirmedCount,
    int MaxPlayers,
    string? UserRsvpStatus, // "Confirmed", "Declined", "Waitlist", null
    bool IsOpenForRsvp);

public record QuickStatsDto(
    int ActivePlayersCount,
    int MatchesThisMonth,
    int TotalSeasonGoals,
    decimal PaidInvoicesPercentage);

public record RecentAnnouncementDto(
    Guid Id,
    string Title,
    string Summary,
    DateTime PublishedAt,
    string Category,
    string AuthorName);

public record DashboardSummaryDto(
    NextMatchWidgetDto? NextMatch,
    QuickStatsDto Stats,
    IReadOnlyList<RecentAnnouncementDto> Announcements);
