using System;
using System.Collections.Generic;
using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Reports;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Reports;

public class TopScorersWidgetTests : TestContext
{
    [Fact]
    public void TopScorersWidget_ShouldRenderScorersTableWithMedals()
    {
        // Arrange
        var scorers = new List<TopScorerEntryDto>
        {
            new(Guid.NewGuid(), "Zico", "Galinho", null, 10, 5, 8, 1.25),
            new(Guid.NewGuid(), "Romário", "Baixinho", null, 8, 3, 7, 1.14),
            new(Guid.NewGuid(), "Pelé", "Rei", null, 6, 4, 8, 0.75)
        };

        // Act
        var cut = RenderComponent<TopScorersWidget>(parameters => parameters
            .Add(p => p.Scorers, scorers));

        // Assert
        cut.Find("[data-testid='scorers-title']").TextContent.Should().Contain("Artilharia & Assistências");
        cut.Find("[data-testid='scorers-table']").Should().NotBeNull();
        cut.Markup.Should().Contain("🥇");
        cut.Markup.Should().Contain("🥈");
        cut.Markup.Should().Contain("🥉");
        cut.Markup.Should().Contain("Zico");
        cut.Markup.Should().Contain("Romário");
    }
}
