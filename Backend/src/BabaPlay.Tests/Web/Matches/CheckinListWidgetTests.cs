using System;
using System.Collections.Generic;
using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Matches;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Matches;

public class CheckinListWidgetTests : TestContext
{
    [Fact]
    public void CheckinListWidget_ShouldRenderConfirmedPlayersList()
    {
        // Arrange
        var list = new List<PlayerRsvpDetailDto>
        {
            new(Guid.NewGuid(), "Zico Santos", "Galinho", null, "Meia", 10, "Confirmed", DateTime.Now, null),
            new(Guid.NewGuid(), "Romário Farias", "Baixinho", null, "Atacante", 11, "Confirmed", DateTime.Now, null)
        };

        // Act
        var cut = RenderComponent<CheckinListWidget>(parameters => parameters
            .Add(p => p.RsvpList, list));

        // Assert
        cut.Markup.Should().Contain("Zico Santos");
        cut.Markup.Should().Contain("Romário Farias");
        cut.Markup.Should().Contain("Meia");
        cut.Markup.Should().Contain("Atacante");
        cut.Markup.Should().Contain("#10");
        cut.Markup.Should().Contain("#11");
    }

    [Fact]
    public void CheckinListWidget_WhenListIsEmpty_ShouldRenderEmptyState()
    {
        // Act
        var cut = RenderComponent<CheckinListWidget>(parameters => parameters
            .Add(p => p.RsvpList, new List<PlayerRsvpDetailDto>()));

        // Assert
        cut.Find("[data-testid='empty-rsvp-list']").Should().NotBeNull();
        cut.Markup.Should().Contain("Nenhum atleta nesta categoria");
    }
}
