using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class MatchEventTypeTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateActiveType()
    {
        var type = MatchEventType.Create(Guid.NewGuid(), "goal", "Goal", 2, true);

        type.Id.Should().NotBeEmpty();
        type.Code.Should().Be("goal");
        type.NormalizedCode.Should().Be("GOAL");
        type.Name.Should().Be("Goal");
        type.Points.Should().Be(2);
        type.IsSystemDefault.Should().BeTrue();
        type.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_EmptyTenant_ShouldThrowValidationException()
    {
        var act = () => MatchEventType.Create(Guid.Empty, "goal", "Goal", 2, false);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Update_ValidData_ShouldChangeFields()
    {
        var type = MatchEventType.Create(Guid.NewGuid(), "goal", "Goal", 2, true);

        type.Update("assist", "Assist", 1);

        type.Code.Should().Be("assist");
        type.NormalizedCode.Should().Be("ASSIST");
        type.Name.Should().Be("Assist");
        type.Points.Should().Be(1);
        type.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_Twice_ShouldBeIdempotent()
    {
        var type = MatchEventType.Create(Guid.NewGuid(), "goal", "Goal", 2, true);
        type.Deactivate();

        var act = () => type.Deactivate();

        act.Should().NotThrow();
        type.IsActive.Should().BeFalse();
    }
}
