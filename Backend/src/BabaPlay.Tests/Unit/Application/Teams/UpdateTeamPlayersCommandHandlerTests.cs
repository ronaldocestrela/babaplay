using BabaPlay.Application.Commands.Teams;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Teams;

public class UpdateTeamPlayersCommandHandlerTests
{
    private readonly Mock<ITeamRepository> _teamRepo = new();
    private readonly Mock<IPlayerRepository> _playerRepo = new();
    private readonly Mock<IPositionRepository> _positionRepo = new();
    private readonly UpdateTeamPlayersCommandHandler _handler;

    public UpdateTeamPlayersCommandHandlerTests()
    {
        _handler = new UpdateTeamPlayersCommandHandler(_teamRepo.Object, _playerRepo.Object, _positionRepo.Object);
    }

    [Fact]
    public async Task Handle_TeamNotFound_ShouldReturnTeamNotFound()
    {
        _teamRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Team?)null);

        var result = await _handler.HandleAsync(new UpdateTeamPlayersCommand(Guid.NewGuid(), []));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TEAM_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_DuplicatePlayers_ShouldReturnValidationError()
    {
        var team = Team.Create(Guid.NewGuid(), "Blue", 3);
        var playerId = Guid.NewGuid();
        _teamRepo.Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>())).ReturnsAsync(team);

        var result = await _handler.HandleAsync(new UpdateTeamPlayersCommand(team.Id, [playerId, playerId]));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TEAM_DUPLICATE_PLAYERS");
    }

    [Fact]
    public async Task Handle_NoGoalkeeper_ShouldReturnGoalkeeperRequired()
    {
        var team = Team.Create(Guid.NewGuid(), "Blue", 3);
        var player = Player.Create(Guid.NewGuid(), "Player 1", null, null, null);
        var position = Position.Create(Guid.NewGuid(), "ATA", "Atacante", null);
        player.SetPositions([position.Id]);

        _teamRepo.Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>())).ReturnsAsync(team);
        _playerRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([player]);
        _positionRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([position]);

        var result = await _handler.HandleAsync(new UpdateTeamPlayersCommand(team.Id, [player.Id]));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TEAM_GOALKEEPER_REQUIRED");
    }

    [Fact]
    public async Task Handle_ValidRosterWithGoalkeeper_ShouldUpdateTeam()
    {
        var team = Team.Create(Guid.NewGuid(), "Blue", 3);
        var goalkeeper = Player.Create(Guid.NewGuid(), "Goalkeeper", null, null, null);
        var goalkeeperPosition = Position.Create(Guid.NewGuid(), "GOLEIRO", "Goleiro", null);
        goalkeeper.SetPositions([goalkeeperPosition.Id]);

        _teamRepo.Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>())).ReturnsAsync(team);
        _playerRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([goalkeeper]);
        _positionRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([goalkeeperPosition]);

        var result = await _handler.HandleAsync(new UpdateTeamPlayersCommand(team.Id, [goalkeeper.Id]));

        result.IsSuccess.Should().BeTrue();
        result.Value!.PlayerIds.Should().ContainSingle().Which.Should().Be(goalkeeper.Id);
        _teamRepo.Verify(r => r.UpdateAsync(It.IsAny<Team>(), It.IsAny<CancellationToken>()), Times.Once);
        _teamRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}