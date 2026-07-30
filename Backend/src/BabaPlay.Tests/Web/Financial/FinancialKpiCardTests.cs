using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Financial;

namespace BabaPlay.Tests.Web.Financial;

public class FinancialKpiCardTests : TestContext
{
    [Fact]
    public void FinancialKpiCard_ShouldRenderTitleAmountAndBadge()
    {
        // Act
        var cut = RenderComponent<FinancialKpiCard>(parameters => parameters
            .Add(p => p.Title, "Saldo da Caixinha")
            .Add(p => p.Amount, 1500.50m)
            .Add(p => p.Icon, "💰")
            .Add(p => p.Subtitle, "Balanço acumulado")
            .Add(p => p.BadgeText, "Superávit")
            .Add(p => p.BadgeColorClass, "badge-success")
            .Add(p => p.ValueColor, "success"));

        // Assert
        cut.Markup.Should().Contain("Saldo da Caixinha");
        cut.Markup.Should().Contain("1.500,50");
        cut.Markup.Should().Contain("💰");
        cut.Markup.Should().Contain("Balanço acumulado");
        cut.Markup.Should().Contain("Superávit");
        cut.Markup.Should().Contain("badge-success");
    }

    [Fact]
    public void FinancialKpiCard_WhenDeficit_ShouldRenderDangerBadge()
    {
        // Act
        var cut = RenderComponent<FinancialKpiCard>(parameters => parameters
            .Add(p => p.Title, "Saldo da Caixinha")
            .Add(p => p.Amount, -200.00m)
            .Add(p => p.BadgeText, "Déficit")
            .Add(p => p.BadgeColorClass, "badge-danger")
            .Add(p => p.ValueColor, "danger"));

        // Assert
        cut.Markup.Should().Contain("Déficit");
        cut.Markup.Should().Contain("badge-danger");
    }
}
