using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Dashboard;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;
using DomainMatch = BabaPlay.Domain.Entities.Match;

namespace BabaPlay.Tests.Unit.Application.Dashboard;

public class GetDashboardSummaryQueryHandlerTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AuthorId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid PlayerId = Guid.NewGuid();

    private readonly Mock<IPlayerRepository> _playerRepo = new();
    private readonly Mock<IMatchRepository> _matchRepo = new();
    private readonly Mock<IGameDayRepository> _gameDayRepo = new();
    private readonly Mock<IAnnouncementRepository> _announcementRepo = new();
    private readonly Mock<IPlayerScoreRepository> _playerScoreRepo = new();
    private readonly Mock<IPlayerMonthlyFeeRepository> _monthlyFeeRepo = new();
    private readonly Mock<ICheckinRepository> _checkinRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();

    public GetDashboardSummaryQueryHandlerTests()
    {
        _tenantContext.Setup(x => x.TenantId).Returns(TenantId);

        _announcementRepo
            .Setup(x => x.GetByTenantAsync(TenantId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Announcement>());

        _playerScoreRepo
            .Setup(x => x.GetAllActiveForRebuildAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerScore>());

        _monthlyFeeRepo
            .Setup(x => x.GetByCompetenceAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerMonthlyFee>());
    }

    [Fact]
    public async Task HandleAsync_WithEmptyTenant_ShouldReturnZerosAndEmptyAnnouncements()
    {
        _playerRepo
            .Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Player>());

        _matchRepo
            .Setup(x => x.GetAllActiveAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainMatch>());

        _gameDayRepo
            .Setup(x => x.GetAllActiveAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameDay>());

        var handler = CreateHandler();

        var result = await handler.HandleAsync(new GetDashboardSummaryQuery());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Stats.ActivePlayersCount.Should().Be(0);
        result.Value.Stats.TotalSeasonGoals.Should().Be(0);
        result.Value.Stats.PaidInvoicesPercentage.Should().Be(0);
        result.Value.Announcements.Should().BeEmpty();
        result.Value.NextMatch.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithRealData_ShouldReturnMappedValues()
    {
        var player = Player.Create(TenantId, UserId, "Atleta Teste", "Pelé", "(11) 99999-9999", null);
        var upcomingGameDay = GameDay.Create(
            TenantId,
            "Baba de Terça",
            DateTime.UtcNow.AddDays(3),
            "Campo Central",
            null,
            20);

        var announcement = Announcement.Create(TenantId, AuthorId, "Aviso Importante", "Conteúdo do aviso publicado");
        announcement.Publish();

        var playerScore = PlayerScore.Create(TenantId, PlayerId);
        playerScore.ApplyDelta(new ScoreBreakdown(0, 0, 0, 5, 0, 0));

        var monthlyFee = PlayerMonthlyFee.Create(
            TenantId,
            PlayerId,
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            100m,
            DateTime.UtcNow.AddDays(10),
            "Mensalidade");
        monthlyFee.ApplyPayment(50m, DateTime.UtcNow);

        _playerRepo
            .Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Player> { player });

        _matchRepo
            .Setup(x => x.GetAllActiveAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainMatch>());

        _gameDayRepo
            .Setup(x => x.GetAllActiveAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameDay> { upcomingGameDay });

        _announcementRepo
            .Setup(x => x.GetByTenantAsync(TenantId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Announcement> { announcement });

        _playerScoreRepo
            .Setup(x => x.GetAllActiveForRebuildAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerScore> { playerScore });

        _monthlyFeeRepo
            .Setup(x => x.GetByCompetenceAsync(DateTime.UtcNow.Year, DateTime.UtcNow.Month, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerMonthlyFee> { monthlyFee });

        _checkinRepo
            .Setup(x => x.CountActiveByGameDayAsync(upcomingGameDay.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        _checkinRepo
            .Setup(x => x.ExistsActiveByPlayerAndGameDayAsync(player.Id, upcomingGameDay.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _userRepo
            .Setup(x => x.FindByIdAsync(AuthorId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAuthDto(AuthorId.ToString(), "diretoria@babaplay.com", true));

        var handler = CreateHandler();

        var result = await handler.HandleAsync(new GetDashboardSummaryQuery(UserId.ToString()));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Stats.ActivePlayersCount.Should().Be(1);
        result.Value.Stats.TotalSeasonGoals.Should().Be(5);
        result.Value.Stats.PaidInvoicesPercentage.Should().Be(50m);
        result.Value.Announcements.Should().ContainSingle();
        result.Value.Announcements[0].Title.Should().Be("Aviso Importante");
        result.Value.Announcements[0].AuthorName.Should().Be("diretoria@babaplay.com");
        result.Value.NextMatch.Should().NotBeNull();
        result.Value.NextMatch!.ConfirmedCount.Should().Be(3);
        result.Value.NextMatch.UserRsvpStatus.Should().Be("Confirmed");
        result.Value.NextMatch.IsOpenForRsvp.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasNoCheckin_ShouldReturnNullRsvpStatus()
    {
        var player = Player.Create(TenantId, UserId, "Atleta Teste", "Pelé", "(11) 99999-9999", null);
        var upcomingGameDay = GameDay.Create(
            TenantId,
            "Baba de Terça",
            DateTime.UtcNow.AddDays(3),
            "Campo Central",
            null,
            20);

        _playerRepo
            .Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Player> { player });

        _matchRepo
            .Setup(x => x.GetAllActiveAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainMatch>());

        _gameDayRepo
            .Setup(x => x.GetAllActiveAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameDay> { upcomingGameDay });

        _checkinRepo
            .Setup(x => x.CountActiveByGameDayAsync(upcomingGameDay.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _checkinRepo
            .Setup(x => x.ExistsActiveByPlayerAndGameDayAsync(player.Id, upcomingGameDay.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = CreateHandler();

        var result = await handler.HandleAsync(new GetDashboardSummaryQuery(UserId.ToString()));

        result.IsSuccess.Should().BeTrue();
        result.Value!.NextMatch.Should().NotBeNull();
        result.Value.NextMatch!.ConfirmedCount.Should().Be(0);
        result.Value.NextMatch.UserRsvpStatus.Should().BeNull();
    }

    private GetDashboardSummaryQueryHandler CreateHandler()
        => new(
            _playerRepo.Object,
            _matchRepo.Object,
            _gameDayRepo.Object,
            _announcementRepo.Object,
            _playerScoreRepo.Object,
            _monthlyFeeRepo.Object,
            _checkinRepo.Object,
            _userRepo.Object,
            _tenantContext.Object);
}
