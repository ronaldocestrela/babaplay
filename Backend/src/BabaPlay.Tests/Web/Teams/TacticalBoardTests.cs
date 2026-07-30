using System;
using System.Collections.Generic;
using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Teams;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Teams;

public class TacticalBoardTests : TestContext
{
    [Fact]
    public void TacticalBoard_ShouldRenderSoccerPitchAndPlayerPins()
    {
        // Arrange
        var positions = new List<PlayerTacticalPositionDto>
        {
            new(Guid.NewGuid(), "Goleiro Taffarel", "Goleiro", 1, 50, 90, "#f59e0b", false),
            new(Guid.NewGuid(), "Zagueiro Aldair", "Zagueiro", 3, 50, 75, "#f59e0b", false)
        };

        var formation = new TacticalFormationDto("Futsal 2-2", "Futsal", positions);

        // Act
        var cut = RenderComponent<TacticalBoard>(parameters => parameters
            .Add(p => p.Formation, formation));

        // Assert
        cut.Find("[data-testid='formation-title']").TextContent.Should().Contain("Futsal 2-2");
        cut.Find("[data-testid='soccer-pitch']").Should().NotBeNull();
        cut.Markup.Should().Contain("Goleiro Taffarel");
        cut.Markup.Should().Contain("Zagueiro Aldair");
    }
}
