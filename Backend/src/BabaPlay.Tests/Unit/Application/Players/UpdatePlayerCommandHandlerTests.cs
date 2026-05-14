using BabaPlay.Application.Commands.Players;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Players;

public class UpdatePlayerCommandHandlerTests
{
    private readonly Mock<IPlayerRepository> _playerRepo = new();
    private readonly UpdatePlayerCommandHandler _handler;

    public UpdatePlayerCommandHandlerTests()
    {
        _handler = new UpdatePlayerCommandHandler(_playerRepo.Object);
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
        var result = await _handler.HandleAsync(
            new UpdatePlayerCommand(unknownId, "New Name", null, null, null));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("PLAYER_NOT_FOUND");
        _playerRepo.Verify(r => r.UpdateAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmptyName_ShouldReturnInvalidName()
    {
        // Arrange
        var player = Player.Create(Guid.NewGuid(), "Original", null, null, null);
        _playerRepo
            .Setup(r => r.GetByIdAsync(player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);

        // Act
        var result = await _handler.HandleAsync(
            new UpdatePlayerCommand(player.Id, "", null, null, null));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_NAME");
        _playerRepo.Verify(r => r.UpdateAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateAndReturnResponse()
    {
        // Arrange
        var player = Player.Create(Guid.NewGuid(), "Old Name", null, null, null);
        _playerRepo
            .Setup(r => r.GetByIdAsync(player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);

        var dob = new DateOnly(1988, 7, 4);

        // Act
        var result = await _handler.HandleAsync(
            new UpdatePlayerCommand(player.Id, "New Name", "Nick", "11955555555", dob));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("New Name");
        result.Value.Nickname.Should().Be("Nick");
        result.Value.Phone.Should().Be("11955555555");
        result.Value.DateOfBirth.Should().Be(dob);
        _playerRepo.Verify(r => r.UpdateAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()), Times.Once);
        _playerRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldPreservePlayerIdAndUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var player = Player.Create(userId, "Original", null, null, null);
        _playerRepo
            .Setup(r => r.GetByIdAsync(player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);

        // Act
        var result = await _handler.HandleAsync(
            new UpdatePlayerCommand(player.Id, "Updated Name", null, null, null));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(player.Id);
        result.Value.UserId.Should().Be(userId);
    }
}
