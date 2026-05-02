using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class PlayerPositionTests
{
    [Fact]
    public void Create_ValidData_ReturnsAssociation()
    {
        var playerId = Guid.NewGuid();
        var positionId = Guid.NewGuid();

        var playerPosition = PlayerPosition.Create(playerId, positionId);

        playerPosition.PlayerId.Should().Be(playerId);
        playerPosition.PositionId.Should().Be(positionId);
    }

    [Fact]
    public void Create_EmptyPlayerId_ThrowsValidationException()
    {
        var act = () => PlayerPosition.Create(Guid.Empty, Guid.NewGuid());

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_EmptyPositionId_ThrowsValidationException()
    {
        var act = () => PlayerPosition.Create(Guid.NewGuid(), Guid.Empty);

        act.Should().Throw<ValidationException>();
    }
}
