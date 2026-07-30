using System;
using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Matches;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Matches;

public class ScoreboardWidgetTests : TestContext
{
    [Fact]
    public void ScoreboardWidget_ShouldRenderTeamsAndScore()
    {
        // Arrange
        var scoreboard = new MatchScoreboardDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Time Amarelo 🟡",
            Guid.NewGuid(),
            "Time Azul 🔵",
            3,
            2,
            "Em Andamento"
        );

        // Act
        var cut = RenderComponent<ScoreboardWidget>(parameters => parameters
            .Add(p => p.Scoreboard, scoreboard));

        // Assert
        cut.Find("[data-testid='home-team-name']").TextContent.Should().Contain("Time Amarelo");
        cut.Find("[data-testid='away-team-name']").TextContent.Should().Contain("Time Azul");
        cut.Find("[data-testid='scoreboard-score']").TextContent.Should().Contain("3");
        cut.Find("[data-testid='scoreboard-score']").TextContent.Should().Contain("2");
        cut.Find("[data-testid='match-status']").TextContent.Should().Contain("Em Andamento");
    }
}
