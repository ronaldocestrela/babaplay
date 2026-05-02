using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class PlayerTests
{
    // ── Create ────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ValidData_ReturnsActivePlayer()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var player = Player.Create(userId, "João Silva", "Jão", "11999999999", new DateOnly(1990, 5, 15));

        // Assert
        player.Id.Should().NotBeEmpty();
        player.UserId.Should().Be(userId);
        player.Name.Should().Be("João Silva");
        player.Nickname.Should().Be("Jão");
        player.Phone.Should().Be("11999999999");
        player.DateOfBirth.Should().Be(new DateOnly(1990, 5, 15));
        player.IsActive.Should().BeTrue();
        player.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        player.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_NullableFieldsOmitted_ReturnsPlayer()
    {
        // Act
        var player = Player.Create(Guid.NewGuid(), "Carlos", null, null, null);

        // Assert
        player.Name.Should().Be("Carlos");
        player.Nickname.Should().BeNull();
        player.Phone.Should().BeNull();
        player.DateOfBirth.Should().BeNull();
    }

    [Fact]
    public void Create_WhitespaceName_ThrowsValidationException()
    {
        // Act
        var act = () => Player.Create(Guid.NewGuid(), "   ", null, null, null);

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_EmptyName_ThrowsValidationException()
    {
        // Act
        var act = () => Player.Create(Guid.NewGuid(), "", null, null, null);

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_EmptyGuidUserId_ThrowsValidationException()
    {
        // Act
        var act = () => Player.Create(Guid.Empty, "Valid Name", null, null, null);

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_TrimsNameAndNickname()
    {
        // Act
        var player = Player.Create(Guid.NewGuid(), "  João  ", "  Jão  ", null, null);

        // Assert
        player.Name.Should().Be("João");
        player.Nickname.Should().Be("Jão");
    }

    // ── Update ────────────────────────────────────────────────────────────

    [Fact]
    public void Update_ValidData_ChangesProperties()
    {
        // Arrange
        var player = Player.Create(Guid.NewGuid(), "Old Name", null, null, null);

        // Act
        player.Update("New Name", "Nick", "11988888888", new DateOnly(1985, 3, 10));

        // Assert
        player.Name.Should().Be("New Name");
        player.Nickname.Should().Be("Nick");
        player.Phone.Should().Be("11988888888");
        player.DateOfBirth.Should().Be(new DateOnly(1985, 3, 10));
        player.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_EmptyName_ThrowsValidationException()
    {
        // Arrange
        var player = Player.Create(Guid.NewGuid(), "Valid Name", null, null, null);

        // Act
        var act = () => player.Update("", null, null, null);

        // Assert
        act.Should().Throw<ValidationException>();
    }

    // ── Deactivate ────────────────────────────────────────────────────────

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        // Arrange
        var player = Player.Create(Guid.NewGuid(), "Player", null, null, null);

        // Act
        player.Deactivate();

        // Assert
        player.IsActive.Should().BeFalse();
        player.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_AlreadyInactive_IsIdempotent()
    {
        // Arrange
        var player = Player.Create(Guid.NewGuid(), "Player", null, null, null);
        player.Deactivate();

        // Act — second deactivation
        var act = () => player.Deactivate();

        // Assert
        act.Should().NotThrow();
        player.IsActive.Should().BeFalse();
    }
}
