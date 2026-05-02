using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Players;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Players;

public class GetPlayerQueryHandlerTests
{
    private readonly Mock<IPlayerRepository> _playerRepo = new();
    private readonly GetPlayerQueryHandler _handler;

    public GetPlayerQueryHandlerTests()
    {
        _handler = new GetPlayerQueryHandler(_playerRepo.Object);
    }

    [Fact]
    public async Task Handle_ExistingPlayer_ShouldReturnPlayerResponse()
    {
        // Arrange
        var player = Player.Create(Guid.NewGuid(), "Carlos Drummond", "Dru", "11977777777", new DateOnly(1985, 3, 10));
        _playerRepo
            .Setup(r => r.GetByIdAsync(player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);

        // Act
        var result = await _handler.HandleAsync(new GetPlayerQuery(player.Id));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(player.Id);
        result.Value.UserId.Should().Be(player.UserId);
        result.Value.Name.Should().Be("Carlos Drummond");
        result.Value.Nickname.Should().Be("Dru");
        result.Value.IsActive.Should().BeTrue();
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
        var result = await _handler.HandleAsync(new GetPlayerQuery(unknownId));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("PLAYER_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ExistingPlayer_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dob = new DateOnly(1992, 8, 20);
        var player = Player.Create(userId, "Ana Lima", null, null, dob);
        _playerRepo
            .Setup(r => r.GetByIdAsync(player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);

        // Act
        var result = await _handler.HandleAsync(new GetPlayerQuery(player.Id));

        // Assert
        result.Value!.UserId.Should().Be(userId);
        result.Value.Nickname.Should().BeNull();
        result.Value.Phone.Should().BeNull();
        result.Value.DateOfBirth.Should().Be(dob);
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
