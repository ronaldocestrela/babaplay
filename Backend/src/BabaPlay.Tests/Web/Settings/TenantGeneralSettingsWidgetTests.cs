using System;
using System.Threading;
using Bunit;
using FluentAssertions;
using Moq;
using Xunit;
using BabaPlay.Web.Components.Settings;
using BabaPlay.Web.Models;
using BabaPlay.Web.Services.Http;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Tests.Web.Settings;

public class TenantGeneralSettingsWidgetTests : TestContext
{
    [Fact]
    public void TenantGeneralSettingsWidget_ShouldRenderInitialValues()
    {
        // Arrange
        var mockService = new Mock<ITenantApiService>();
        Services.AddSingleton(mockService.Object);

        var initialSettings = new TenantSettingsDto(
            Guid.NewGuid(),
            "Associação Futebol & Resenha",
            "futebol-resenha",
            12,
            "/logos/logo.png",
            "Rua das Palmeiras",
            "45",
            "Pituba",
            "Salvador",
            "BA",
            "41810-000",
            -13.001,
            -38.455);

        // Act
        var cut = RenderComponent<TenantGeneralSettingsWidget>(parameters =>
            parameters.Add(p => p.InitialSettings, initialSettings));

        // Assert
        cut.Find("[data-testid='input-tenant-name']").GetAttribute("value").Should().Be("Associação Futebol & Resenha");
        cut.Find("[data-testid='input-players-per-team']").GetAttribute("value").Should().Be("12");
        cut.Find("[data-testid='input-street']").GetAttribute("value").Should().Be("Rua das Palmeiras");
        cut.Find("[data-testid='input-city']").GetAttribute("value").Should().Be("Salvador");
    }

    [Fact]
    public void TenantGeneralSettingsWidget_OnSubmit_ShouldCallUpdateSettingsAsync()
    {
        // Arrange
        var mockService = new Mock<ITenantApiService>();
        var updated = new TenantSettingsDto(
            Guid.NewGuid(),
            "Associação Futebol & Resenha Atualizada",
            "futebol-resenha",
            10,
            null, null, null, null, "Salvador", "BA", null, null, null);

        mockService
            .Setup(x => x.UpdateSettingsAsync(It.IsAny<UpdateTenantSettingsDto>(), It.IsAny<IBrowserFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        Services.AddSingleton(mockService.Object);

        var initialSettings = new TenantSettingsDto(
            Guid.NewGuid(),
            "Associação Futebol & Resenha",
            "futebol-resenha",
            12,
            null, null, null, null, "Salvador", "BA", null, null, null);

        var cut = RenderComponent<TenantGeneralSettingsWidget>(parameters =>
            parameters.Add(p => p.InitialSettings, initialSettings));

        // Act
        cut.Find("form").Submit();

        // Assert
        mockService.Verify(x => x.UpdateSettingsAsync(It.IsAny<UpdateTenantSettingsDto>(), It.IsAny<IBrowserFile>(), It.IsAny<CancellationToken>()), Times.Once);
        cut.Find("[data-testid='settings-success-alert']").Should().NotBeNull();
    }
}
