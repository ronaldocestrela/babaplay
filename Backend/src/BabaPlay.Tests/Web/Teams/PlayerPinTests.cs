using System;
using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Teams;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Teams;

public class PlayerPinTests : TestContext
{
    [Fact]
    public void PlayerPin_ShouldRenderPinWithCoordinatesAndJerseyNumber()
    {
        // Arrange
        var player = new PlayerTacticalPositionDto(
            Guid.NewGuid(),
            "Rivaldo Ferreira",
            "Meia",
            10,
            45.5,
            60.0,
            "#3b82f6",
            false
        );

        // Act
        var cut = RenderComponent<PlayerPin>(parameters => parameters
            .Add(p => p.Player, player));

        // Assert
        cut.Find("[data-testid='pin-circle']").TextContent.Should().Contain("#10");
        cut.Find("[data-testid='pin-name']").TextContent.Should().Contain("Rivaldo Ferreira");
        cut.Markup.Should().Contain("left: 45.5%");
        cut.Markup.Should().Contain("top: 60%");
    }
}
