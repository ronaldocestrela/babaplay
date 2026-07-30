using System;
using System.Collections.Generic;
using System.Threading;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using BabaPlay.Web.Models;
using BabaPlay.Web.Pages.Financial;
using BabaPlay.Web.Services.Http;

namespace BabaPlay.Tests.Web.Financial;

public class FinancialDashboardPageTests : TestContext
{
    [Fact]
    public void FinancialDashboardPage_ShouldRenderOverviewData()
    {
        // Arrange
        var mockService = new Mock<IFinancialApiService>();
        var overviewDto = new FinancialOverviewDto
        {
            Year = 2026,
            Month = 5,
            TotalIncome = 1200m,
            TotalExpense = 400m,
            NetBalance = 800m,
            PendingMonthlyFeesAmount = 150m,
            DelinquentPlayersCount = 2,
            CollectionRatePercentage = 85.5m,
            RecentTransactions = new List<RecentTransactionDto>
            {
                new RecentTransactionDto
                {
                    Id = Guid.NewGuid(),
                    TenantId = Guid.NewGuid(),
                    Type = 1,
                    Amount = 100m,
                    Description = "Mensalidade Pedro",
                    OccurredOnUtc = new DateTime(2026, 5, 5, 0, 0, 0, DateTimeKind.Utc)
                }
            }
        };

        mockService
            .Setup(x => x.GetFinancialOverviewAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(overviewDto);

        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<FinancialDashboard>();

        // Assert
        cut.Markup.Should().Contain("Dashboard Financeiro");
        cut.Markup.Should().Contain("800,00"); // NetBalance
        cut.Markup.Should().Contain("1.200,00"); // TotalIncome
        cut.Markup.Should().Contain("400,00"); // TotalExpense
        cut.Markup.Should().Contain("150,00"); // PendingMonthlyFees
        cut.Markup.Should().Contain("2 atletas em atraso");
        cut.Markup.Should().Contain("85"); // CollectionRatePercentage
        cut.Markup.Should().Contain("%");
        cut.Markup.Should().Contain("Mensalidade Pedro");
    }

    [Fact]
    public void FinancialDashboardPage_WhenServiceReturnsNull_ShouldShowErrorBanner()
    {
        // Arrange
        var mockService = new Mock<IFinancialApiService>();
        mockService
            .Setup(x => x.GetFinancialOverviewAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FinancialOverviewDto?)null);

        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<FinancialDashboard>();

        // Assert
        cut.Find("[data-testid='financial-error-banner']").Should().NotBeNull();
        cut.Markup.Should().Contain("Não foi possível carregar os dados financeiros");
    }
}
