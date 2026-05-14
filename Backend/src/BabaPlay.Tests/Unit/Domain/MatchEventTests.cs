using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class MatchEventTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateActiveEvent()
    {
        var ev = MatchEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            45,
            "Great play");

        ev.Id.Should().NotBeEmpty();
        ev.Minute.Should().Be(45);
        ev.Notes.Should().Be("Great play");
        ev.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_InvalidMinute_ShouldThrowValidationException()
    {
        var act = () => MatchEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            131,
            null);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Update_ValidData_ShouldUpdateFields()
    {
        var ev = MatchEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            15,
            null);

        var newTypeId = Guid.NewGuid();
        ev.Update(newTypeId, 90, "Updated");

        ev.MatchEventTypeId.Should().Be(newTypeId);
        ev.Minute.Should().Be(90);
        ev.Notes.Should().Be("Updated");
        ev.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_Twice_ShouldBeIdempotent()
    {
        var ev = MatchEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            10,
            null);

        ev.Deactivate();
        var act = () => ev.Deactivate();

        act.Should().NotThrow();
        ev.IsActive.Should().BeFalse();
    }
}
