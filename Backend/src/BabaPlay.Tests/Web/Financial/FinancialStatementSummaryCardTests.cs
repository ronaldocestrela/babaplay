using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Financial;

namespace BabaPlay.Tests.Web.Financial;

public class FinancialStatementSummaryCardTests : TestContext
{
    [Fact]
    public void FinancialStatementSummaryCard_WithSurplus_ShouldRenderSuperavitBadge()
    {
        // Act
        var cut = RenderComponent<FinancialStatementSummaryCard>(parameters => parameters
            .Add(p => p.TotalIncomes, 1000m)
            .Add(p => p.TotalExpenses, 400m)
            .Add(p => p.NetBalance, 600m));

        // Assert
        cut.Find("[data-testid='val-total-incomes']").TextContent.Should().Contain("1.000,00");
        cut.Find("[data-testid='val-total-expenses']").TextContent.Should().Contain("400,00");
        cut.Find("[data-testid='val-net-balance']").TextContent.Should().Contain("600,00");
        cut.Find("[data-testid='badge-superavit']").TextContent.Should().Contain("Superávit");
    }

    [Fact]
    public void FinancialStatementSummaryCard_WithDeficit_ShouldRenderDeficitBadge()
    {
        // Act
        var cut = RenderComponent<FinancialStatementSummaryCard>(parameters => parameters
            .Add(p => p.TotalIncomes, 300m)
            .Add(p => p.TotalExpenses, 800m)
            .Add(p => p.NetBalance, -500m));

        // Assert
        cut.Find("[data-testid='badge-deficit']").TextContent.Should().Contain("Déficit");
    }
}
