using BabaPlay.Application.Commands.Matches;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;
using DomainMatch = BabaPlay.Domain.Entities.Match;

namespace BabaPlay.Tests.Unit.Application.Matches;

public class UpdateMatchCommandHandlerTests
{
    private readonly Mock<IMatchRepository> _matchRepository = new();
    private readonly Mock<IGameDayRepository> _gameDayRepository = new();
    private readonly Mock<ITeamRepository> _teamRepository = new();
    private readonly UpdateMatchCommandHandler _handler;

    public UpdateMatchCommandHandlerTests()
    {
        _handler = new UpdateMatchCommandHandler(_matchRepository.Object, _gameDayRepository.Object, _teamRepository.Object);
    }

    [Fact]
    public async Task Handle_EmptyGameDayId_ShouldReturnInvalidGameDayId()
    {
        var match = DomainMatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);
        var cmd = new UpdateMatchCommand(match.Id, Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), null);

        _matchRepository.Setup(x => x.GetByIdAsync(match.Id, It.IsAny<CancellationToken>())).ReturnsAsync(match);

        var result = await _handler.HandleAsync(cmd);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_GAMEDAY_ID");
    }

    [Fact]
    public async Task Handle_OnlyHomeTeamId_ShouldReturnTeamsPairRequired()
    {
        var match = DomainMatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);
        var cmd = new UpdateMatchCommand(match.Id, Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), null);

        _matchRepository.Setup(x => x.GetByIdAsync(match.Id, It.IsAny<CancellationToken>())).ReturnsAsync(match);

        var result = await _handler.HandleAsync(cmd);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_TEAMS_PAIR_REQUIRED");
    }

    [Fact]
    public async Task Handle_OnlyAwayTeamId_ShouldReturnTeamsPairRequired()
    {
        var match = DomainMatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);
        var cmd = new UpdateMatchCommand(match.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, null);

        _matchRepository.Setup(x => x.GetByIdAsync(match.Id, It.IsAny<CancellationToken>())).ReturnsAsync(match);

        var result = await _handler.HandleAsync(cmd);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_TEAMS_PAIR_REQUIRED");
    }

    [Fact]
    public async Task Handle_SameTeams_ShouldReturnTeamsMustBeDifferent()
    {
        var match = DomainMatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);
        var teamId = Guid.NewGuid();
        var cmd = new UpdateMatchCommand(match.Id, Guid.NewGuid(), teamId, teamId, null);

        _matchRepository.Setup(x => x.GetByIdAsync(match.Id, It.IsAny<CancellationToken>())).ReturnsAsync(match);

        var result = await _handler.HandleAsync(cmd);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TEAMS_MUST_BE_DIFFERENT");
    }

    [Fact]
    public async Task Handle_NotFound_ShouldReturnMatchNotFound()
    {
        _matchRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainMatch?)null);

        var result = await _handler.HandleAsync(new UpdateMatchCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldUpdateMatch()
    {
        var match = DomainMatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "old");
        var cmd = new UpdateMatchCommand(match.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "new");

        _matchRepository.Setup(x => x.GetByIdAsync(match.Id, It.IsAny<CancellationToken>())).ReturnsAsync(match);
        _gameDayRepository.Setup(x => x.GetByIdAsync(cmd.GameDayId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(GameDay.Create(Guid.NewGuid(), "Rodada", DateTime.UtcNow.AddHours(2), null, null, 22));
        _teamRepository.Setup(x => x.GetByIdAsync(cmd.HomeTeamId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Team.Create(Guid.NewGuid(), "Casa", 11));
        _teamRepository.Setup(x => x.GetByIdAsync(cmd.AwayTeamId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Team.Create(Guid.NewGuid(), "Visitante", 11));
        _matchRepository.Setup(x => x.ExistsByGameDayAndTeamsAsync(cmd.GameDayId, cmd.HomeTeamId!.Value, cmd.AwayTeamId!.Value, cmd.MatchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(cmd);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Description.Should().Be("new");
        _matchRepository.Verify(x => x.UpdateAsync(match, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GameDayNotFound_ShouldReturnGameDayNotFound()
    {
        var match = DomainMatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);
        var cmd = new UpdateMatchCommand(match.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);

        _matchRepository.Setup(x => x.GetByIdAsync(match.Id, It.IsAny<CancellationToken>())).ReturnsAsync(match);
        _gameDayRepository.Setup(x => x.GetByIdAsync(cmd.GameDayId, It.IsAny<CancellationToken>())).ReturnsAsync((GameDay?)null);

        var result = await _handler.HandleAsync(cmd);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("GAMEDAY_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_PastGameDay_ShouldReturnGameDayPast()
    {
        var match = DomainMatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);
        var cmd = new UpdateMatchCommand(match.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);

        var pastGameDay = GameDay.Create(Guid.NewGuid(), "Rodada", DateTime.UtcNow.AddHours(2), null, null, 22);
        typeof(GameDay).GetProperty(nameof(GameDay.ScheduledAt))!
            .SetValue(pastGameDay, DateTime.UtcNow.AddHours(-1));

        _matchRepository.Setup(x => x.GetByIdAsync(match.Id, It.IsAny<CancellationToken>())).ReturnsAsync(match);
        _gameDayRepository.Setup(x => x.GetByIdAsync(cmd.GameDayId, It.IsAny<CancellationToken>())).ReturnsAsync(pastGameDay);

        var result = await _handler.HandleAsync(cmd);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("GAMEDAY_PAST");
    }

    [Fact]
    public async Task Handle_DuplicateMatch_ShouldReturnMatchAlreadyExists()
    {
        var match = DomainMatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);
        var cmd = new UpdateMatchCommand(match.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);

        _matchRepository.Setup(x => x.GetByIdAsync(match.Id, It.IsAny<CancellationToken>())).ReturnsAsync(match);
        _gameDayRepository.Setup(x => x.GetByIdAsync(cmd.GameDayId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(GameDay.Create(Guid.NewGuid(), "Rodada", DateTime.UtcNow.AddHours(2), null, null, 22));
        _teamRepository.Setup(x => x.GetByIdAsync(cmd.HomeTeamId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Team.Create(Guid.NewGuid(), "Casa", 11));
        _teamRepository.Setup(x => x.GetByIdAsync(cmd.AwayTeamId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Team.Create(Guid.NewGuid(), "Visitante", 11));
        _matchRepository.Setup(x => x.ExistsByGameDayAndTeamsAsync(cmd.GameDayId, cmd.HomeTeamId!.Value, cmd.AwayTeamId!.Value, cmd.MatchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(cmd);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_ALREADY_EXISTS");
    }

    [Fact]
    public async Task Handle_WithoutFixedTeams_ShouldUpdateMatch()
    {
        var match = DomainMatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "old");
        var cmd = new UpdateMatchCommand(match.Id, Guid.NewGuid(), null, null, "novo sem times");

        _matchRepository.Setup(x => x.GetByIdAsync(match.Id, It.IsAny<CancellationToken>())).ReturnsAsync(match);
        _gameDayRepository.Setup(x => x.GetByIdAsync(cmd.GameDayId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(GameDay.Create(Guid.NewGuid(), "Rodada", DateTime.UtcNow.AddHours(2), null, null, 22));
        _matchRepository.Setup(x => x.ExistsByGameDayAsync(cmd.GameDayId, cmd.MatchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(cmd);

        result.IsSuccess.Should().BeTrue();
        result.Value!.HomeTeamId.Should().Be(Guid.Empty);
        result.Value.AwayTeamId.Should().Be(Guid.Empty);
    }
}