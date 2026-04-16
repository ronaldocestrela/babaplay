using BabaPlay.Modules.Associates.Entities;
using BabaPlay.Modules.CheckIns.Entities;
using BabaPlay.Modules.MatchReports.Dtos;
using BabaPlay.Modules.MatchReports.Entities;
using BabaPlay.Modules.MatchReports.Services;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using BabaPlay.Tests.Unit.Helpers;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Services;

public sealed class MatchReportServiceTests
{
    private readonly List<CheckInSession> _sessions = [];
    private readonly List<Associate> _associates = [];
    private readonly List<MatchReport> _reports = [];
    private readonly List<MatchReportGame> _games = [];
    private readonly List<MatchReportPlayerStat> _playerStats = [];
    private readonly Mock<ITenantRepository<CheckInSession>> _sessionRepo = new();
    private readonly Mock<ITenantRepository<Associate>> _associateRepo = new();
    private readonly Mock<ITenantRepository<MatchReport>> _reportRepo = new();
    private readonly Mock<ITenantRepository<MatchReportGame>> _gameRepo = new();
    private readonly Mock<ITenantRepository<MatchReportPlayerStat>> _playerStatRepo = new();
    private readonly Mock<ITenantUnitOfWork> _uow = new();
    private readonly MatchReportService _sut;

    public MatchReportServiceTests()
    {
        _sessionRepo.Setup(x => x.Query()).Returns(() => _sessions.AsAsyncQueryable());
        _sessionRepo.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken _) => _sessions.FirstOrDefault(x => x.Id == id));

        _associateRepo.Setup(x => x.Query()).Returns(() => _associates.AsAsyncQueryable());

        _reportRepo.Setup(x => x.Query()).Returns(() => _reports.AsAsyncQueryable());
        _reportRepo.Setup(x => x.AddAsync(It.IsAny<MatchReport>(), It.IsAny<CancellationToken>()))
            .Callback<MatchReport, CancellationToken>((entity, _) => _reports.Add(entity))
            .Returns(Task.CompletedTask);
        _reportRepo.Setup(x => x.Update(It.IsAny<MatchReport>()));

        _gameRepo.Setup(x => x.Query()).Returns(() => _games.AsAsyncQueryable());
        _gameRepo.Setup(x => x.AddAsync(It.IsAny<MatchReportGame>(), It.IsAny<CancellationToken>()))
            .Callback<MatchReportGame, CancellationToken>((entity, _) => _games.Add(entity))
            .Returns(Task.CompletedTask);
        _gameRepo.Setup(x => x.Remove(It.IsAny<MatchReportGame>()))
            .Callback<MatchReportGame>(entity => _games.Remove(entity));

        _playerStatRepo.Setup(x => x.Query()).Returns(() => _playerStats.AsAsyncQueryable());
        _playerStatRepo.Setup(x => x.AddAsync(It.IsAny<MatchReportPlayerStat>(), It.IsAny<CancellationToken>()))
            .Callback<MatchReportPlayerStat, CancellationToken>((entity, _) => _playerStats.Add(entity))
            .Returns(Task.CompletedTask);
        _playerStatRepo.Setup(x => x.Remove(It.IsAny<MatchReportPlayerStat>()))
            .Callback<MatchReportPlayerStat>(entity => _playerStats.Remove(entity));

        _uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _sut = new MatchReportService(
            _sessionRepo.Object,
            _associateRepo.Object,
            _reportRepo.Object,
            _gameRepo.Object,
            _playerStatRepo.Object,
            _uow.Object);
    }

    [Fact]
    public async Task UpsertAsync_CreatesDraftReportWithGamesAndStats()
    {
        var session = new CheckInSession { Id = "session-1" };
        var associate = new Associate { Id = "associate-1", Name = "Jogador 1", IsActive = true };
        _sessions.Add(session);
        _associates.Add(associate);

        var result = await _sut.UpsertAsync(
            session.Id,
            " Rodada de quarta ",
            [new MatchReportGameInput(
                "Jogo 1",
                "Primeiro tempo intenso",
                [new MatchReportPlayerStatInput("associate-1", 2, 1, 1, 0, "Capitao")])],
            "user-1",
            isAdmin: false,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.SessionId.Should().Be(session.Id);
        result.Value.Notes.Should().Be("Rodada de quarta");
        result.Value.Status.Should().Be(MatchReportStatus.Draft);
        result.Value.Games.Should().HaveCount(1);
        result.Value.Games[0].GameNumber.Should().Be(1);
        result.Value.Games[0].PlayerStats.Should().ContainSingle();
        result.Value.Games[0].PlayerStats[0].Goals.Should().Be(2);
        _reportRepo.Verify(x => x.AddAsync(It.IsAny<MatchReport>(), It.IsAny<CancellationToken>()), Times.Once);
        _gameRepo.Verify(x => x.AddAsync(It.IsAny<MatchReportGame>(), It.IsAny<CancellationToken>()), Times.Once);
        _playerStatRepo.Verify(x => x.AddAsync(It.IsAny<MatchReportPlayerStat>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpsertAsync_NegativeStats_ReturnsInvalid()
    {
        _sessions.Add(new CheckInSession { Id = "session-1" });

        var result = await _sut.UpsertAsync(
            "session-1",
            null,
            [new MatchReportGameInput("Jogo 1", null, [new MatchReportPlayerStatInput("associate-1", -1, 0, 0, 0, null)])],
            "user-1",
            isAdmin: false,
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.Errors.Should().Contain(x => x.Contains("non-negative", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task UpsertAsync_UnknownAssociate_ReturnsInvalid()
    {
        _sessions.Add(new CheckInSession { Id = "session-1" });

        var result = await _sut.UpsertAsync(
            "session-1",
            null,
            [new MatchReportGameInput("Jogo 1", null, [new MatchReportPlayerStatInput("associate-404", 1, 0, 0, 0, null)])],
            "user-1",
            isAdmin: false,
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.Error.Should().Contain("Associates not found");
    }

    [Fact]
    public async Task UpsertAsync_FinalizedReportAndUserIsNotAdmin_ReturnsForbidden()
    {
        _sessions.Add(new CheckInSession { Id = "session-1" });
        _associates.Add(new Associate { Id = "associate-1", Name = "Jogador 1", IsActive = true });
        _reports.Add(new MatchReport
        {
            Id = "report-1",
            SessionId = "session-1",
            Status = MatchReportStatus.Finalized,
            FinalizedAt = DateTime.UtcNow,
            FinalizedByUserId = "admin-1"
        });

        var result = await _sut.UpsertAsync(
            "session-1",
            null,
            [new MatchReportGameInput("Jogo 1", null, [new MatchReportPlayerStatInput("associate-1", 1, 0, 0, 0, null)])],
            "user-1",
            isAdmin: false,
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task UpsertAsync_FinalizedReportAndUserIsAdmin_ReplacesGamesAndKeepsFinalizedState()
    {
        _sessions.Add(new CheckInSession { Id = "session-1" });
        _associates.Add(new Associate { Id = "associate-1", Name = "Jogador 1", IsActive = true });
        _associates.Add(new Associate { Id = "associate-2", Name = "Jogador 2", IsActive = true });

        var report = new MatchReport
        {
            Id = "report-1",
            SessionId = "session-1",
            Status = MatchReportStatus.Finalized,
            FinalizedAt = DateTime.UtcNow.AddHours(-1),
            FinalizedByUserId = "admin-1"
        };
        var oldGame = new MatchReportGame { Id = "game-1", MatchReportId = report.Id, GameNumber = 1, Title = "Antigo" };
        var oldStat = new MatchReportPlayerStat { Id = "stat-1", MatchReportGameId = oldGame.Id, AssociateId = "associate-1" };
        _reports.Add(report);
        _games.Add(oldGame);
        _playerStats.Add(oldStat);

        var result = await _sut.UpsertAsync(
            "session-1",
            "Atualizada",
            [new MatchReportGameInput(
                "Jogo novo",
                null,
                [new MatchReportPlayerStatInput("associate-2", 0, 2, 0, 1, "Expulso no fim")])],
            "admin-1",
            isAdmin: true,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(MatchReportStatus.Finalized);
        result.Value.Games.Should().ContainSingle();
        result.Value.Games[0].Title.Should().Be("Jogo novo");
        result.Value.Games[0].PlayerStats.Should().ContainSingle(x => x.AssociateId == "associate-2");
        _games.Should().NotContain(x => x.Id == oldGame.Id);
        _playerStats.Should().NotContain(x => x.Id == oldStat.Id);
    }

    [Fact]
    public async Task FinalizeAsync_MarksReportAsFinalized()
    {
        var report = new MatchReport { Id = "report-1", SessionId = "session-1" };
        _reports.Add(report);

        var result = await _sut.FinalizeAsync("session-1", "admin-1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(MatchReportStatus.Finalized);
        result.Value.FinalizedByUserId.Should().Be("admin-1");
        report.Status.Should().Be(MatchReportStatus.Finalized);
    }
}