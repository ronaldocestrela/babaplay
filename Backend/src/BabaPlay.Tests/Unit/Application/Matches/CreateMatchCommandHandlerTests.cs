using BabaPlay.Application.Commands.Matches;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;
using DomainMatch = BabaPlay.Domain.Entities.Match;

namespace BabaPlay.Tests.Unit.Application.Matches;

public class CreateMatchCommandHandlerTests
{
    private readonly Mock<IMatchRepository> _matchRepository = new();
    private readonly Mock<IGameDayRepository> _gameDayRepository = new();
    private readonly Mock<ITeamRepository> _teamRepository = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly CreateMatchCommandHandler _handler;

    public CreateMatchCommandHandlerTests()
    {
        _tenantContext.SetupGet(x => x.TenantId).Returns(Guid.NewGuid());
        _handler = new CreateMatchCommandHandler(
            _matchRepository.Object,
            _gameDayRepository.Object,
            _teamRepository.Object,
            _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_SameTeams_ShouldReturnValidationError()
    {
        var teamId = Guid.NewGuid();

        var result = await _handler.HandleAsync(new CreateMatchCommand(Guid.NewGuid(), teamId, teamId, null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TEAMS_MUST_BE_DIFFERENT");
    }

    [Fact]
    public async Task Handle_GameDayNotFound_ShouldReturnGameDayNotFound()
    {
        _gameDayRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameDay?)null);

        var result = await _handler.HandleAsync(new CreateMatchCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("GAMEDAY_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateMatch()
    {
        var gameDayId = Guid.NewGuid();
        var homeTeamId = Guid.NewGuid();
        var awayTeamId = Guid.NewGuid();

        _gameDayRepository
            .Setup(x => x.GetByIdAsync(gameDayId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(GameDay.Create(Guid.NewGuid(), "Rodada", DateTime.UtcNow.AddHours(2), null, null, 22));

        _teamRepository
            .Setup(x => x.GetByIdAsync(homeTeamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Team.Create(Guid.NewGuid(), "Casa", 11));
        _teamRepository
            .Setup(x => x.GetByIdAsync(awayTeamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Team.Create(Guid.NewGuid(), "Visitante", 11));

        _matchRepository
            .Setup(x => x.ExistsByGameDayAndTeamsAsync(gameDayId, homeTeamId, awayTeamId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(new CreateMatchCommand(gameDayId, homeTeamId, awayTeamId, "Desc"));

        result.IsSuccess.Should().BeTrue();
        _matchRepository.Verify(x => x.AddAsync(It.IsAny<DomainMatch>(), It.IsAny<CancellationToken>()), Times.Once);
        _matchRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PastGameDay_ShouldReturnGameDayPast()
    {
        var gameDayId = Guid.NewGuid();
        var homeTeamId = Guid.NewGuid();
        var awayTeamId = Guid.NewGuid();

        var pastGameDay = GameDay.Create(Guid.NewGuid(), "Rodada passada", DateTime.UtcNow.AddHours(2), null, null, 22);
        pastGameDay.Update("Rodada passada", DateTime.UtcNow.AddHours(3), null, null, 22);

        typeof(GameDay).GetProperty(nameof(GameDay.ScheduledAt))!
            .SetValue(pastGameDay, DateTime.UtcNow.AddHours(-1));

        _gameDayRepository
            .Setup(x => x.GetByIdAsync(gameDayId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pastGameDay);

        var result = await _handler.HandleAsync(new CreateMatchCommand(gameDayId, homeTeamId, awayTeamId, null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("GAMEDAY_PAST");
    }
}