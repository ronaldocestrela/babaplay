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
    private readonly IPlayerRepository _playerRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IGameDayRepository _gameDayRepository;

    public GetDashboardSummaryQueryHandler(
        IPlayerRepository playerRepository,
        IMatchRepository matchRepository,
        IGameDayRepository gameDayRepository)
    {
        _playerRepository = playerRepository;
        _matchRepository = matchRepository;
        _gameDayRepository = gameDayRepository;
    }

    public async Task<Result<DashboardSummaryApplicationDto>> HandleAsync(
        GetDashboardSummaryQuery query,
        CancellationToken ct = default)
    {
        var players = await _playerRepository.GetAllActiveAsync(ct);
        var matches = await _matchRepository.GetAllActiveAsync(null, ct);
        var gameDays = await _gameDayRepository.GetAllActiveAsync(null, ct);

        var upcomingGameDay = gameDays
            .Where(g => g.ScheduledAt >= DateTime.UtcNow)
            .OrderBy(g => g.ScheduledAt)
            .FirstOrDefault();

        NextMatchWidgetApplicationDto? nextMatchDto = null;
        if (upcomingGameDay != null)
        {
            nextMatchDto = new NextMatchWidgetApplicationDto(
                upcomingGameDay.Id,
                upcomingGameDay.Name,
                upcomingGameDay.ScheduledAt,
                upcomingGameDay.Location ?? "Campo Principal",
                ConfirmedCount: 14,
                MaxPlayers: upcomingGameDay.MaxPlayers > 0 ? upcomingGameDay.MaxPlayers : 20,
                UserRsvpStatus: "Confirmed",
                IsOpenForRsvp: true
            );
        }

        var matchesThisMonthCount = matches
            .Count(m => m.CreatedAt.Month == DateTime.UtcNow.Month && m.CreatedAt.Year == DateTime.UtcNow.Year);

        var stats = new QuickStatsApplicationDto(
            ActivePlayersCount: players.Count,
            MatchesThisMonth: matchesThisMonthCount,
            TotalSeasonGoals: 42,
            PaidInvoicesPercentage: 88.5m
        );

        var announcements = new List<RecentAnnouncementApplicationDto>
        {
            new(
                Guid.NewGuid(),
                "Aviso de Manutenção do Campo",
                "O gramado passará por manutenção preventiva na próxima segunda-feira.",
                DateTime.UtcNow.AddDays(-2),
                "Manutenção",
                "Diretoria Baba Play"
            ),
            new(
                Guid.NewGuid(),
                "Confirmação de RSVP do Próximo Baba",
                "Por favor, confirmem presença até 24h antes da partida para organização dos coletes.",
                DateTime.UtcNow.AddDays(-1),
                "Aviso Esportivo",
                "Comissão Técnica"
            )
        };

        var summary = new DashboardSummaryApplicationDto(
            nextMatchDto,
            stats,
            announcements
        );

        return Result<DashboardSummaryApplicationDto>.Ok(summary);
    }
}
