using BabaPlay.Application.Commands.Players;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Players;

public class UpdatePlayerPositionsCommandHandlerTests
{
    private readonly Mock<IPlayerRepository> _playerRepo = new();
    private readonly Mock<IPositionRepository> _positionRepo = new();
    private readonly UpdatePlayerPositionsCommandHandler _handler;

    public UpdatePlayerPositionsCommandHandlerTests()
    {
        _handler = new UpdatePlayerPositionsCommandHandler(_playerRepo.Object, _positionRepo.Object);
    }

    [Fact]
    public async Task Handle_PlayerNotFound_ShouldReturnPlayerNotFound()
    {
        _playerRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Player?)null);

        var result = await _handler.HandleAsync(new UpdatePlayerPositionsCommand(Guid.NewGuid(), [Guid.NewGuid()]));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("PLAYER_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_MoreThanThreePositions_ShouldReturnLimitExceeded()
    {
        var player = Player.Create(Guid.NewGuid(), "Player", null, null, null);
        _playerRepo.Setup(r => r.GetByIdAsync(player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);

        var result = await _handler.HandleAsync(new UpdatePlayerPositionsCommand(
            player.Id,
            [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("POSITIONS_LIMIT_EXCEEDED");
    }

    [Fact]
    public async Task Handle_UnknownPosition_ShouldReturnPositionNotFound()
    {
        var player = Player.Create(Guid.NewGuid(), "Player", null, null, null);
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };

        _playerRepo.Setup(r => r.GetByIdAsync(player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);
        _positionRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([Position.Create(Guid.NewGuid(), "GK", "Goleiro", null)]);

        var result = await _handler.HandleAsync(new UpdatePlayerPositionsCommand(player.Id, ids));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("POSITION_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_DuplicatePositions_ShouldReturnDuplicatePositions()
    {
        var player = Player.Create(Guid.NewGuid(), "Player", null, null, null);
        var repeated = Guid.NewGuid();

        _playerRepo.Setup(r => r.GetByIdAsync(player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);

        var result = await _handler.HandleAsync(new UpdatePlayerPositionsCommand(player.Id, [repeated, repeated]));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("DUPLICATE_POSITIONS");
        _positionRepo.Verify(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmptyPositionId_ShouldReturnInvalidPositionId()
    {
        var player = Player.Create(Guid.NewGuid(), "Player", null, null, null);

        _playerRepo.Setup(r => r.GetByIdAsync(player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);

        var result = await _handler.HandleAsync(new UpdatePlayerPositionsCommand(player.Id, [Guid.Empty]));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_POSITION_ID");
        _positionRepo.Verify(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldReplacePlayerPositions()
    {
        var tenantId = Guid.NewGuid();
        var player = Player.Create(Guid.NewGuid(), "Player", null, null, null);
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };

        _playerRepo.Setup(r => r.GetByIdAsync(player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);
        _positionRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Position>
            {
                Position.Create(tenantId, "GK", "Goleiro", null),
                Position.Create(tenantId, "CM", "Meia", null),
            });

        var result = await _handler.HandleAsync(new UpdatePlayerPositionsCommand(player.Id, ids));

        result.IsSuccess.Should().BeTrue();
        result.Value!.PositionIds.Should().BeEquivalentTo(ids);
        _playerRepo.Verify(r => r.UpdateAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()), Times.Once);
        _playerRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
