using System;
using System.Collections.Generic;
using System.Threading;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using BabaPlay.Web.Models;
using BabaPlay.Web.Pages.Dashboard;
using BabaPlay.Web.Services.Http;

namespace BabaPlay.Tests.Web.Dashboard;

public class DashboardPageTests : TestContext
{
    [Fact]
    public void DashboardPage_ShouldRenderSummaryData()
    {
        // Arrange
        this.AddTestAuthorization().SetAuthorized("Test User");

        var mockService = new Mock<IDashboardApiService>();
        var summaryDto = new DashboardSummaryDto(
            new NextMatchWidgetDto(Guid.NewGuid(), "Baba de Sábado", DateTime.Now.AddDays(3), "Arena Sol", 15, 20, "Confirmed", true),
            new QuickStatsDto(30, 4, 50, 90.0m),
            new List<RecentAnnouncementDto>
            {
                new(Guid.NewGuid(), "Título Aviso", "Resumo do aviso", DateTime.Now, "Geral", "Admin")
            }
        );

        mockService
            .Setup(x => x.GetDashboardSummaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DashboardSummaryLoadResult(summaryDto, null));

        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<BabaPlay.Web.Pages.Dashboard.Dashboard>();

        // Assert
        cut.Markup.Should().Contain("Painel de Controle");
        cut.Markup.Should().Contain("Baba de Sábado");
        cut.Markup.Should().Contain("30");
        cut.Markup.Should().Contain("Título Aviso");
    }

    [Fact]
    public void DashboardPage_WhenServiceReturnsNull_ShouldShowErrorBanner()
    {
        // Arrange
        this.AddTestAuthorization().SetAuthorized("Test User");

        var mockService = new Mock<IDashboardApiService>();
        mockService
            .Setup(x => x.GetDashboardSummaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DashboardSummaryLoadResult(null, "Não foi possível carregar os dados do dashboard. Verifique sua conexão ou tente novamente mais tarde."));

        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<BabaPlay.Web.Pages.Dashboard.Dashboard>();

        // Assert
        cut.Find("[data-testid='dashboard-error-banner']").Should().NotBeNull();
        cut.Markup.Should().Contain("Não foi possível carregar os dados do dashboard");
    }

    [Fact]
    public void DashboardPage_OnInitialLoad_ShouldShowSkeletonBeforeDataArrives()
    {
        // Arrange
        this.AddTestAuthorization().SetAuthorized("Test User");

        var loadGate = new TaskCompletionSource<DashboardSummaryLoadResult>();
        var mockService = new Mock<IDashboardApiService>();
        mockService
            .Setup(x => x.GetDashboardSummaryAsync(It.IsAny<CancellationToken>()))
            .Returns(loadGate.Task);

        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<BabaPlay.Web.Pages.Dashboard.Dashboard>();

        // Assert — skeleton visible while API is pending
        cut.Find("[data-testid='dashboard-skeleton']").Should().NotBeNull();

        loadGate.SetResult(new DashboardSummaryLoadResult(
            new DashboardSummaryDto(
                null,
                new QuickStatsDto(0, 0, 0, 0m),
                Array.Empty<RecentAnnouncementDto>()),
            null));

        cut.WaitForAssertion(() =>
        {
            cut.Find("[data-testid='quick-stats-widget']").Should().NotBeNull();
            cut.FindAll("[data-testid='dashboard-skeleton']").Should().BeEmpty();
        });
    }

    [Fact]
    public void DashboardPage_WhenServiceReturnsUnauthorized_ShouldShowAuthErrorMessage()
    {
        // Arrange
        this.AddTestAuthorization().SetAuthorized("Test User");

        var mockService = new Mock<IDashboardApiService>();
        mockService
            .Setup(x => x.GetDashboardSummaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DashboardSummaryLoadResult(null, "Sessão expirada ou inválida. Faça login novamente."));

        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<BabaPlay.Web.Pages.Dashboard.Dashboard>();

        // Assert
        cut.Find("[data-testid='dashboard-error-banner']").Should().NotBeNull();
        cut.Markup.Should().Contain("Sessão expirada ou inválida");
    }
}
