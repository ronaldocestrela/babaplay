using System;
using System.Collections.Generic;
using System.Threading;
using Bunit;
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
            .ReturnsAsync(summaryDto);

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
        var mockService = new Mock<IDashboardApiService>();
        mockService
            .Setup(x => x.GetDashboardSummaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((DashboardSummaryDto?)null);

        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<BabaPlay.Web.Pages.Dashboard.Dashboard>();

        // Assert
        cut.Find("[data-testid='dashboard-error-banner']").Should().NotBeNull();
        cut.Markup.Should().Contain("Não foi possível carregar os dados do dashboard");
    }
}
