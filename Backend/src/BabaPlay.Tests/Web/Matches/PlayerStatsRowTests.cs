using System;
using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Matches;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Matches;

public class PlayerStatsRowTests : TestContext
{
    [Fact]
    public void PlayerStatsRow_ShouldIncrementAndDecrementGoals()
    {
        // Arrange
        var player = new PlayerMatchStatsDto(
            Guid.NewGuid(),
            "Rivaldo",
            null,
            Guid.NewGuid(),
            Goals: 1,
            Assists: 0,
            YellowCards: 0,
            RedCards: 0,
            OwnGoals: 0
        );

        PlayerMatchStatsDto? updatedStats = null;

        // Act
        var cut = RenderComponent<PlayerStatsRow>(parameters => parameters
            .Add(p => p.Stats, player)
            .Add(p => p.OnStatsChanged, (PlayerMatchStatsDto s) => updatedStats = s));

        // Click increment goal
        var addGoalBtn = cut.Find("[data-testid='btn-add-goal']");
        addGoalBtn.Click();

        // Assert
        updatedStats.Should().NotBeNull();
        updatedStats!.Goals.Should().Be(2);
    }

    [Fact]
    public void PlayerStatsRow_WhenYellowCardIncrementedTwice_ShouldFlagRedCard()
    {
        // Arrange
        var player = new PlayerMatchStatsDto(
            Guid.NewGuid(),
            "Dunga",
            null,
            Guid.NewGuid(),
            Goals: 0,
            Assists: 0,
            YellowCards: 1,
            RedCards: 0,
            OwnGoals: 0
        );

        PlayerMatchStatsDto? updatedStats = null;

        // Act
        var cut = RenderComponent<PlayerStatsRow>(parameters => parameters
            .Add(p => p.Stats, player)
            .Add(p => p.OnStatsChanged, (PlayerMatchStatsDto s) => updatedStats = s));

        // Click increment yellow card
        var addYellowBtn = cut.Find("[data-testid='btn-add-yellow']");
        addYellowBtn.Click();

        // Assert
        updatedStats.Should().NotBeNull();
        updatedStats!.YellowCards.Should().Be(2);
        updatedStats.RedCards.Should().Be(1);
    }
}
