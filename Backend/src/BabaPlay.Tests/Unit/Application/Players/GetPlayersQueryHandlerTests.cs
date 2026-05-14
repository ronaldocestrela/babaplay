using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Players;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Players;

public class GetPlayersQueryHandlerTests
{
    private readonly Mock<IPlayerRepository> _playerRepo = new();
    private readonly GetPlayersQueryHandler _handler;

    public GetPlayersQueryHandlerTests()
    {
        _handler = new GetPlayersQueryHandler(_playerRepo.Object);
    }

    [Fact]
    public async Task Handle_WithPlayers_ShouldReturnAllActivePlayers()
    {
        // Arrange
        var players = new List<Player>
        {
            Player.Create(Guid.NewGuid(), "Player One", null, null, null),
            Player.Create(Guid.NewGuid(), "Player Two", "P2", null, null),
        };
        _playerRepo
            .Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(players);

        // Act
        var result = await _handler.HandleAsync(new GetPlayersQuery());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        result.Value.Select(p => p.Name).Should().Contain("Player One").And.Contain("Player Two");
    }

    [Fact]
    public async Task Handle_EmptyList_ShouldReturnEmptyCollection()
    {
        // Arrange
        _playerRepo
            .Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Player>());

        // Act
        var result = await _handler.HandleAsync(new GetPlayersQuery());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithPlayers_ShouldMapFieldsCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var player = Player.Create(userId, "Lucas Freitas", "Luca", "11966666666", new DateOnly(1995, 1, 1));
        _playerRepo
            .Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Player> { player });

        // Act
        var result = await _handler.HandleAsync(new GetPlayersQuery());

        // Assert
        var dto = result.Value!.Single();
        dto.UserId.Should().Be(userId);
        dto.Name.Should().Be("Lucas Freitas");
        dto.Nickname.Should().Be("Luca");
        dto.Phone.Should().Be("11966666666");
        dto.DateOfBirth.Should().Be(new DateOnly(1995, 1, 1));
        dto.IsActive.Should().BeTrue();
    }
}
