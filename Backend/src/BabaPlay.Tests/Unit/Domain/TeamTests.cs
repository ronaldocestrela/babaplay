using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class TeamTests
{
    [Fact]
    public void Create_ValidData_ReturnsActiveTeam()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var team = Team.Create(tenantId, "Blue Team", 11);

        // Assert
        team.TenantId.Should().Be(tenantId);
        team.Name.Should().Be("Blue Team");
        team.NormalizedName.Should().Be("BLUE TEAM");
        team.MaxPlayers.Should().Be(11);
        team.IsActive.Should().BeTrue();
        team.PlayerIds.Should().BeEmpty();
    }

    [Fact]
    public void Create_InvalidTenantId_ThrowsValidationException()
    {
        var act = () => Team.Create(Guid.Empty, "Blue Team", 11);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_EmptyName_ThrowsValidationException()
    {
        var act = () => Team.Create(Guid.NewGuid(), " ", 11);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_InvalidMaxPlayers_ThrowsValidationException()
    {
        var act = () => Team.Create(Guid.NewGuid(), "Blue Team", 0);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Update_ValidData_UpdatesProperties()
    {
        // Arrange
        var team = Team.Create(Guid.NewGuid(), "Blue Team", 11);

        // Act
        team.Update("Red Team", 7);

        // Assert
        team.Name.Should().Be("Red Team");
        team.NormalizedName.Should().Be("RED TEAM");
        team.MaxPlayers.Should().Be(7);
        team.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_AlreadyInactive_IsIdempotent()
    {
        // Arrange
        var team = Team.Create(Guid.NewGuid(), "Blue Team", 11);
        team.Deactivate();

        // Act
        var act = () => team.Deactivate();

        // Assert
        act.Should().NotThrow();
        team.IsActive.Should().BeFalse();
    }

    [Fact]
    public void SetPlayers_ValidPlayersWithinLimit_ShouldReplacePlayers()
    {
        // Arrange
        var team = Team.Create(Guid.NewGuid(), "Blue Team", 3);
        var playerIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        team.SetPlayers(playerIds, hasGoalkeeper: true);

        // Assert
        team.PlayerIds.Should().BeEquivalentTo(playerIds);
    }

    [Fact]
    public void SetPlayers_AboveMaxPlayers_ShouldThrowValidationException()
    {
        // Arrange
        var team = Team.Create(Guid.NewGuid(), "Blue Team", 2);
        var playerIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var act = () => team.SetPlayers(playerIds, hasGoalkeeper: true);

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void SetPlayers_DuplicatePlayers_ShouldThrowValidationException()
    {
        // Arrange
        var team = Team.Create(Guid.NewGuid(), "Blue Team", 3);
        var playerId = Guid.NewGuid();

        // Act
        var act = () => team.SetPlayers([playerId, playerId], hasGoalkeeper: true);

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void SetPlayers_EmptyPlayerId_ShouldThrowValidationException()
    {
        // Arrange
        var team = Team.Create(Guid.NewGuid(), "Blue Team", 3);

        // Act
        var act = () => team.SetPlayers([Guid.Empty], hasGoalkeeper: true);

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void SetPlayers_NoGoalkeeper_ShouldThrowValidationException()
    {
        // Arrange
        var team = Team.Create(Guid.NewGuid(), "Blue Team", 3);
        var playerIds = new[] { Guid.NewGuid() };

        // Act
        var act = () => team.SetPlayers(playerIds, hasGoalkeeper: false);

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void SetPlayers_Null_ShouldClearPlayers()
    {
        // Arrange
        var team = Team.Create(Guid.NewGuid(), "Blue Team", 3);
        team.SetPlayers([Guid.NewGuid()], hasGoalkeeper: true);

        // Act
        team.SetPlayers(null, hasGoalkeeper: false);

        // Assert
        team.PlayerIds.Should().BeEmpty();
    }
}
