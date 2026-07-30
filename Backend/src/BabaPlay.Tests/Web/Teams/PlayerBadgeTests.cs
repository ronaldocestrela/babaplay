using System;
using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Teams;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Teams;

public class PlayerBadgeTests : TestContext
{
    [Fact]
    public void PlayerBadge_ShouldRenderPlayerDetailsAndStars()
    {
        // Arrange
        var player = new DrawnPlayerDto(
            Guid.NewGuid(),
            "Bebeto Silva",
            "Bebeto",
            "Atacante",
            4,
            null,
            Guid.NewGuid()
        );

        // Act
        var cut = RenderComponent<PlayerBadge>(parameters => parameters
            .Add(p => p.Player, player)
            .Add(p => p.CanMoveRight, true));

        // Assert
        cut.Find("[data-testid='player-name']").TextContent.Should().Contain("Bebeto Silva");
        cut.Find("[data-testid='player-position']").TextContent.Should().Contain("Atacante");
        cut.Find("[data-testid='player-stars']").TextContent.Should().Contain("★★★★");
        cut.Find("[data-testid='btn-move-right']").Should().NotBeNull();
    }
}
