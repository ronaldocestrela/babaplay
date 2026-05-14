using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class TeamPlayerTests
{
    [Fact]
    public void Create_ValidData_ReturnsAssociation()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var playerId = Guid.NewGuid();

        // Act
        var teamPlayer = TeamPlayer.Create(teamId, playerId);

        // Assert
        teamPlayer.TeamId.Should().Be(teamId);
        teamPlayer.PlayerId.Should().Be(playerId);
    }

    [Fact]
    public void Create_EmptyTeamId_ThrowsValidationException()
    {
        // Act
        var act = () => TeamPlayer.Create(Guid.Empty, Guid.NewGuid());

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_EmptyPlayerId_ThrowsValidationException()
    {
        // Act
        var act = () => TeamPlayer.Create(Guid.NewGuid(), Guid.Empty);

        // Assert
        act.Should().Throw<ValidationException>();
    }
}
