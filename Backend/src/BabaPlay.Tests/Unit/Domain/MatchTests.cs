using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class MatchTests
{
    [Fact]
    public void Create_ValidData_ShouldCreatePendingActiveMatch()
    {
        var match = Match.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "  Clássico local  ");

        match.Id.Should().NotBeEmpty();
        match.Status.Should().Be(MatchStatus.Pending);
        match.IsActive.Should().BeTrue();
        match.Description.Should().Be("Clássico local");
    }

    [Fact]
    public void Create_SameTeams_ShouldThrowValidationException()
    {
        var teamId = Guid.NewGuid();

        var act = () => Match.Create(Guid.NewGuid(), Guid.NewGuid(), teamId, teamId, null);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Update_ValidData_ShouldUpdateFields()
    {
        var match = Match.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);

        var newGameDayId = Guid.NewGuid();
        var newHomeTeamId = Guid.NewGuid();
        var newAwayTeamId = Guid.NewGuid();

        match.Update(newGameDayId, newHomeTeamId, newAwayTeamId, "  Updated  ");

        match.GameDayId.Should().Be(newGameDayId);
        match.HomeTeamId.Should().Be(newHomeTeamId);
        match.AwayTeamId.Should().Be(newAwayTeamId);
        match.Description.Should().Be("Updated");
        match.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void ChangeStatus_ValidFlow_ShouldTransition()
    {
        var match = Match.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);

        match.ChangeStatus(MatchStatus.Scheduled);
        match.ChangeStatus(MatchStatus.InProgress);
        match.ChangeStatus(MatchStatus.Completed);

        match.Status.Should().Be(MatchStatus.Completed);
    }

    [Fact]
    public void ChangeStatus_InvalidTransition_ShouldThrowValidationException()
    {
        var match = Match.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);

        var act = () => match.ChangeStatus(MatchStatus.Completed);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Deactivate_Twice_ShouldBeIdempotent()
    {
        var match = Match.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);
        match.Deactivate();

        var act = () => match.Deactivate();

        act.Should().NotThrow();
        match.IsActive.Should().BeFalse();
    }
}