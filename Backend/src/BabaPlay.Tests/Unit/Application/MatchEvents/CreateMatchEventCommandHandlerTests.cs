using BabaPlay.Application.Commands.MatchEvents;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using FluentAssertions;
using Moq;
using DomainMatch = BabaPlay.Domain.Entities.Match;

namespace BabaPlay.Tests.Unit.Application.MatchEvents;

public class CreateMatchEventCommandHandlerTests
{
    private readonly Mock<IMatchEventRepository> _eventRepository = new();
    private readonly Mock<IMatchEventTypeRepository> _typeRepository = new();
    private readonly Mock<IMatchRepository> _matchRepository = new();
    private readonly Mock<ITeamRepository> _teamRepository = new();
    private readonly Mock<IPlayerRepository> _playerRepository = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly Mock<IMatchEventRealtimeNotifier> _realtimeNotifier = new();
    private readonly CreateMatchEventCommandHandler _handler;

    public CreateMatchEventCommandHandlerTests()
    {
        _tenantContext.SetupGet(x => x.TenantId).Returns(Guid.NewGuid());
        _handler = new CreateMatchEventCommandHandler(
            _eventRepository.Object,
            _typeRepository.Object,
            _matchRepository.Object,
            _teamRepository.Object,
            _playerRepository.Object,
            _tenantContext.Object,
            _realtimeNotifier.Object);
    }

    [Fact]
    public async Task Handle_MatchNotFound_ShouldReturnNotFound()
    {
        _matchRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainMatch?)null);

        var result = await _handler.HandleAsync(new CreateMatchEventCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 20, null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_EVENT_MATCH_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateEvent()
    {
        var matchId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var typeId = Guid.NewGuid();

        var match = DomainMatch.Create(Guid.NewGuid(), Guid.NewGuid(), teamId, Guid.NewGuid(), "Match");
        match.ChangeStatus(MatchStatus.Scheduled);
        _matchRepository
            .Setup(x => x.GetByIdAsync(matchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(match);

        var activeType = MatchEventType.Create(Guid.NewGuid(), "goal", "Goal", 2, true);
        typeId = activeType.Id;
        _typeRepository
            .Setup(x => x.GetByIdAsync(typeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeType);

        var team = Team.Create(Guid.NewGuid(), "Team A", 11);
        team.SetPlayers([playerId], hasGoalkeeper: true);
        _teamRepository
            .Setup(x => x.GetByIdAsync(teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        _playerRepository
            .Setup(x => x.GetByIdAsync(playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Player.Create(Guid.NewGuid(), "Player", null, null, null));

        var result = await _handler.HandleAsync(new CreateMatchEventCommand(
            matchId, teamId, playerId, typeId, 20, "goal"));

        result.IsSuccess.Should().BeTrue();
        _eventRepository.Verify(x => x.AddAsync(It.IsAny<MatchEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _realtimeNotifier.Verify(x => x.NotifyMatchEventCreatedAsync(matchId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PlayerNotInTeam_ShouldReturnValidationError()
    {
        var matchId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var typeId = Guid.NewGuid();

        var match = DomainMatch.Create(Guid.NewGuid(), Guid.NewGuid(), teamId, Guid.NewGuid(), "Match");
        match.ChangeStatus(MatchStatus.Scheduled);
        _matchRepository
            .Setup(x => x.GetByIdAsync(matchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(match);

        var activeType = MatchEventType.Create(Guid.NewGuid(), "goal", "Goal", 2, true);
        _typeRepository
            .Setup(x => x.GetByIdAsync(typeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeType);

        _teamRepository
            .Setup(x => x.GetByIdAsync(teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Team.Create(Guid.NewGuid(), "Team A", 11));

        _playerRepository
            .Setup(x => x.GetByIdAsync(playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Player.Create(Guid.NewGuid(), "Player", null, null, null));

        var result = await _handler.HandleAsync(new CreateMatchEventCommand(
            matchId, teamId, playerId, typeId, 20, null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_EVENT_PLAYER_NOT_IN_TEAM");
    }
}
