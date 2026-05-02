using BabaPlay.Application.Commands.Players;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Players;

public class DeletePlayerCommandHandlerTests
{
    private readonly Mock<IPlayerRepository> _playerRepo = new();
    private readonly DeletePlayerCommandHandler _handler;

    public DeletePlayerCommandHandlerTests()
    {
        _handler = new DeletePlayerCommandHandler(_playerRepo.Object);
    }

    [Fact]
    public async Task Handle_PlayerNotFound_ShouldReturnPlayerNotFound()
    {
        // Arrange
        var unknownId = Guid.NewGuid();
        _playerRepo
            .Setup(r => r.GetByIdAsync(unknownId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Player?)null);

        // Act
        var result = await _handler.HandleAsync(new DeletePlayerCommand(unknownId));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("PLAYER_NOT_FOUND");
        _playerRepo.Verify(r => r.UpdateAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ActivePlayer_ShouldDeactivateAndReturnSuccess()
    {
        // Arrange
        var player = Player.Create(Guid.NewGuid(), "Marcos Vinicius", null, null, null);
        _playerRepo
            .Setup(r => r.GetByIdAsync(player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);

        // Act
        var result = await _handler.HandleAsync(new DeletePlayerCommand(player.Id));

        // Assert
        result.IsSuccess.Should().BeTrue();
        player.IsActive.Should().BeFalse();
        _playerRepo.Verify(r => r.UpdateAsync(player, It.IsAny<CancellationToken>()), Times.Once);
        _playerRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AlreadyDeactivatedPlayer_ShouldStillSucceed()
    {
        // Arrange
        var player = Player.Create(Guid.NewGuid(), "Inactive Player", null, null, null);
        player.Deactivate();
        _playerRepo
            .Setup(r => r.GetByIdAsync(player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);

        // Act
        var result = await _handler.HandleAsync(new DeletePlayerCommand(player.Id));

        // Assert — idempotent
        result.IsSuccess.Should().BeTrue();
        player.IsActive.Should().BeFalse();
    }
}
