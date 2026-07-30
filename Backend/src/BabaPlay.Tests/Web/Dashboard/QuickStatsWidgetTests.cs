using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Dashboard;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Dashboard;

public class QuickStatsWidgetTests : TestContext
{
    [Fact]
    public void QuickStatsWidget_ShouldRenderMetricsValues()
    {
        // Arrange
        var stats = new QuickStatsDto(25, 6, 84, 92.5m);

        // Act
        var cut = RenderComponent<QuickStatsWidget>(parameters => parameters
            .Add(p => p.Stats, stats));

        // Assert
        cut.Markup.Should().Contain("25");
        cut.Markup.Should().Contain("6");
        cut.Markup.Should().Contain("84");
        cut.Markup.Should().Contain("92.5%");
    }
}
