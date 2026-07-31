using System;
using System.Collections.Generic;
using System.Threading;
using Bunit;
using FluentAssertions;
using Moq;
using Xunit;
using BabaPlay.Web.Models;
using BabaPlay.Web.Pages.Settings;
using BabaPlay.Web.Services.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Tests.Web.Settings;

public class TenantSettingsPageTests : TestContext
{
    [Fact]
    public void TenantSettingsPage_ShouldRenderTabsAndLoadSettings()
    {
        // Arrange
        var mockService = new Mock<ITenantApiService>();
        var settings = new TenantSettingsDto(
            Guid.NewGuid(),
            "Baba das Galáxias FC",
            "baba-galaxias",
            10,
            "/logos/baba.png",
            "Rua Principal",
            "100",
            "Centro",
            "Salvador",
            "BA",
            "40000-000",
            -12.9714,
            -38.5014);

        mockService
            .Setup(x => x.GetSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);

        mockService
            .Setup(x => x.GetGameDayOptionsAsync(It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TenantGameDayOptionDto>());

        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<TenantSettings>();

        // Assert
        cut.Markup.Should().Contain("Configurações da Associação");
        cut.Find("[data-testid='tab-general']").Should().NotBeNull();
        cut.Find("[data-testid='tab-gamedays']").Should().NotBeNull();
        cut.Find("[data-testid='tab-invite']").Should().NotBeNull();
        cut.Markup.Should().Contain("Baba das Galáxias FC");
    }

    [Fact]
    public void TenantSettingsPage_WhenTabSwitched_ShouldRenderCorrespondingWidget()
    {
        // Arrange
        var mockService = new Mock<ITenantApiService>();
        var settings = new TenantSettingsDto(
            Guid.NewGuid(),
            "Liga dos Amigos",
            "liga-amigos",
            8,
            null, null, null, null, "Recife", "PE", null, null, null);

        mockService
            .Setup(x => x.GetSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);

        mockService
            .Setup(x => x.GetGameDayOptionsAsync(It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TenantGameDayOptionDto>
            {
                new(Guid.NewGuid(), settings.Id, DayOfWeek.Saturday, new TimeOnly(9, 0), true, DateTime.UtcNow, null)
            });

        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<TenantSettings>();

        // Act - Clique na aba Dias Fixos
        cut.Find("[data-testid='tab-gamedays']").Click();

        // Assert
        cut.Find("[data-testid='tenant-game-day-options-widget']").Should().NotBeNull();
        cut.Markup.Should().Contain("Sábado");

        // Act - Clique na aba Convite
        cut.Find("[data-testid='tab-invite']").Click();

        // Assert
        cut.Find("[data-testid='tenant-invite-link-widget']").Should().NotBeNull();
    }
}
