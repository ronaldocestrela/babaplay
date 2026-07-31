using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Dashboard;

public sealed class GetDashboardSummaryQueryHandler
    : IQueryHandler<GetDashboardSummaryQuery, Result<DashboardSummaryApplicationDto>>
{
    private const int RecentAnnouncementsLimit = 5;
    private const int AnnouncementSummaryMaxLength = 120;

    private readonly IPlayerRepository _playerRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IGameDayRepository _gameDayRepository;
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly IPlayerScoreRepository _playerScoreRepository;
    private readonly IPlayerMonthlyFeeRepository _monthlyFeeRepository;
    private readonly ICheckinRepository _checkinRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITenantContext _tenantContext;

    public GetDashboardSummaryQueryHandler(
        IPlayerRepository playerRepository,
        IMatchRepository matchRepository,
        IGameDayRepository gameDayRepository,
        IAnnouncementRepository announcementRepository,
        IPlayerScoreRepository playerScoreRepository,
        IPlayerMonthlyFeeRepository monthlyFeeRepository,
        ICheckinRepository checkinRepository,
        IUserRepository userRepository,
        ITenantContext tenantContext)
    {
        _playerRepository = playerRepository;
        _matchRepository = matchRepository;
        _gameDayRepository = gameDayRepository;
        _announcementRepository = announcementRepository;
        _playerScoreRepository = playerScoreRepository;
        _monthlyFeeRepository = monthlyFeeRepository;
        _checkinRepository = checkinRepository;
        _userRepository = userRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<DashboardSummaryApplicationDto>> HandleAsync(
        GetDashboardSummaryQuery query,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var tenantId = _tenantContext.TenantId;

        var players = await _playerRepository.GetAllActiveAsync(ct);
        var matches = await _matchRepository.GetAllActiveAsync(null, ct);
        var gameDays = await _gameDayRepository.GetAllActiveAsync(null, ct);
        var scores = await _playerScoreRepository.GetAllActiveForRebuildAsync(null, ct);
        var monthlyFees = await _monthlyFeeRepository.GetByCompetenceAsync(now.Year, now.Month, ct);
        var publishedAnnouncements = await _announcementRepository.GetByTenantAsync(tenantId, onlyPublished: true, ct);

        var upcomingGameDay = gameDays
            .Where(g => g.ScheduledAt >= now)
            .OrderBy(g => g.ScheduledAt)
            .FirstOrDefault();

        NextMatchWidgetApplicationDto? nextMatchDto = null;
        if (upcomingGameDay != null)
        {
            var confirmedCount = await _checkinRepository.CountActiveByGameDayAsync(upcomingGameDay.Id, ct);
            var userRsvpStatus = await ResolveUserRsvpStatusAsync(
                query.RequestingUserId,
                players,
                upcomingGameDay.Id,
                ct);

            nextMatchDto = new NextMatchWidgetApplicationDto(
                upcomingGameDay.Id,
                upcomingGameDay.Name,
                upcomingGameDay.ScheduledAt,
                upcomingGameDay.Location ?? "Campo Principal",
                ConfirmedCount: confirmedCount,
                MaxPlayers: upcomingGameDay.MaxPlayers > 0 ? upcomingGameDay.MaxPlayers : 20,
                UserRsvpStatus: userRsvpStatus,
                IsOpenForRsvp: upcomingGameDay.ScheduledAt >= now
            );
        }

        var matchesThisMonthCount = matches
            .Count(m => m.CreatedAt.Month == now.Month && m.CreatedAt.Year == now.Year);

        var totalMonthlyFees = monthlyFees.Sum(x => x.Amount);
        var paidMonthlyFees = monthlyFees.Sum(x => x.PaidAmount);
        var paidInvoicesPercentage = totalMonthlyFees > 0
            ? Math.Round((paidMonthlyFees / totalMonthlyFees) * 100m, 2)
            : 0m;

        var stats = new QuickStatsApplicationDto(
            ActivePlayersCount: players.Count,
            MatchesThisMonth: matchesThisMonthCount,
            TotalSeasonGoals: scores.Sum(s => s.Goals),
            PaidInvoicesPercentage: paidInvoicesPercentage
        );

        var announcements = await MapRecentAnnouncementsAsync(publishedAnnouncements, ct);

        var summary = new DashboardSummaryApplicationDto(
            nextMatchDto,
            stats,
            announcements
        );

        return Result<DashboardSummaryApplicationDto>.Ok(summary);
    }

    private async Task<string?> ResolveUserRsvpStatusAsync(
        string? requestingUserId,
        IReadOnlyList<Domain.Entities.Player> players,
        Guid gameDayId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(requestingUserId) || !Guid.TryParse(requestingUserId, out var userId))
            return null;

        var currentPlayer = players.FirstOrDefault(p => p.UserId == userId);
        if (currentPlayer == null)
            return null;

        var hasCheckin = await _checkinRepository.ExistsActiveByPlayerAndGameDayAsync(
            currentPlayer.Id,
            gameDayId,
            ct);

        return hasCheckin ? "Confirmed" : null;
    }

    private async Task<IReadOnlyList<RecentAnnouncementApplicationDto>> MapRecentAnnouncementsAsync(
        IReadOnlyList<Domain.Entities.Announcement> announcements,
        CancellationToken ct)
    {
        var recent = announcements
            .OrderByDescending(a => a.PublishedAtUtc ?? a.CreatedAt)
            .Take(RecentAnnouncementsLimit)
            .ToList();

        var mapped = new List<RecentAnnouncementApplicationDto>(recent.Count);

        foreach (var announcement in recent)
        {
            var author = await _userRepository.FindByIdAsync(announcement.AuthorId.ToString(), ct);
            var authorName = author?.Email ?? "Diretoria";

            mapped.Add(new RecentAnnouncementApplicationDto(
                announcement.Id,
                announcement.Title,
                TruncateContent(announcement.Content),
                announcement.PublishedAtUtc ?? announcement.CreatedAt,
                ResolveCategory(announcement.Tags),
                authorName));
        }

        return mapped;
    }

    private static string TruncateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        var trimmed = content.Trim();
        if (trimmed.Length <= AnnouncementSummaryMaxLength)
            return trimmed;

        return trimmed[..AnnouncementSummaryMaxLength].TrimEnd() + "...";
    }

    private static string ResolveCategory(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
            return "Geral";

        var firstTag = tags
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();

        return string.IsNullOrWhiteSpace(firstTag) ? "Geral" : firstTag;
    }
}
