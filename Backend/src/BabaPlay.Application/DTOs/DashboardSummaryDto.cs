using System;
using System.Collections.Generic;

namespace BabaPlay.Application.DTOs;

public record NextMatchWidgetApplicationDto(
    Guid MatchId,
    string Title,
    DateTime Date,
    string Location,
    int ConfirmedCount,
    int MaxPlayers,
    string? UserRsvpStatus,
    bool IsOpenForRsvp);

public record QuickStatsApplicationDto(
    int ActivePlayersCount,
    int MatchesThisMonth,
    int TotalSeasonGoals,
    decimal PaidInvoicesPercentage);

public record RecentAnnouncementApplicationDto(
    Guid Id,
    string Title,
    string Summary,
    DateTime PublishedAt,
    string Category,
    string AuthorName);

public record DashboardSummaryApplicationDto(
    NextMatchWidgetApplicationDto? NextMatch,
    QuickStatsApplicationDto Stats,
    IReadOnlyList<RecentAnnouncementApplicationDto> Announcements);
