using System;
using System.Collections.Generic;
using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Teams;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Teams;

public class TeamColumnTests : TestContext
{
    [Fact]
    public void TeamColumn_ShouldRenderTeamHeaderAndAverageStars()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var players = new List<DrawnPlayerDto>
        {
            new(Guid.NewGuid(), "Jogador 1", null, "Meia", 5, null, teamId),
            new(Guid.NewGuid(), "Jogador 2", null, "Zagueiro", 3, null, teamId)
        };

        var team = new DrawnTeamDto(
            teamId,
            "Time Amarelo 🟡",
            "#f59e0b",
            "bg-warning text-dark",
            players,
            4.0
        );

        // Act
        var cut = RenderComponent<TeamColumn>(parameters => parameters
            .Add(p => p.Team, team));

        // Assert
        cut.Find("[data-testid='team-name']").TextContent.Should().Contain("Time Amarelo");
        cut.Find("[data-testid='team-count']").TextContent.Should().Contain("2 atletas escalados");
        cut.Markup.Should().Contain("4.0");
    }
}
