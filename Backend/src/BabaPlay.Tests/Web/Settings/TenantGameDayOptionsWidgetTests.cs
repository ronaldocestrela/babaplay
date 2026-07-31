using System;
using System.Collections.Generic;
using System.Threading;
using Bunit;
using FluentAssertions;
using Moq;
using Xunit;
using BabaPlay.Web.Components.Settings;
using BabaPlay.Web.Models;
using BabaPlay.Web.Services.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Tests.Web.Settings;

public class TenantGameDayOptionsWidgetTests : TestContext
{
    [Fact]
    public void TenantGameDayOptionsWidget_ShouldRenderOptionsList()
    {
        // Arrange
        var mockService = new Mock<ITenantApiService>();
        var tenantId = Guid.NewGuid();
        var options = new List<TenantGameDayOptionDto>
        {
            new(Guid.NewGuid(), tenantId, DayOfWeek.Saturday, new TimeOnly(8, 0), true, DateTime.UtcNow, null),
            new(Guid.NewGuid(), tenantId, DayOfWeek.Wednesday, new TimeOnly(19, 30), false, DateTime.UtcNow, null)
        };

        mockService
            .Setup(x => x.GetGameDayOptionsAsync(It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(options);

        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<TenantGameDayOptionsWidget>();

        // Assert
        cut.Markup.Should().Contain("Sábado");
        cut.Markup.Should().Contain("08:00");
        cut.Markup.Should().Contain("Quarta-feira");
        cut.Markup.Should().Contain("19:30");
    }

    [Fact]
    public void TenantGameDayOptionsWidget_WhenStatusToggled_ShouldCallChangeGameDayOptionStatusAsync()
    {
        // Arrange
        var mockService = new Mock<ITenantApiService>();
        var tenantId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var options = new List<TenantGameDayOptionDto>
        {
            new(optionId, tenantId, DayOfWeek.Sunday, new TimeOnly(7, 30), true, DateTime.UtcNow, null)
        };

        mockService
            .Setup(x => x.GetGameDayOptionsAsync(It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(options);

        var updated = new TenantGameDayOptionDto(optionId, tenantId, DayOfWeek.Sunday, new TimeOnly(7, 30), false, DateTime.UtcNow, DateTime.UtcNow);

        mockService
            .Setup(x => x.ChangeGameDayOptionStatusAsync(optionId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<TenantGameDayOptionsWidget>();

        // Act
        var toggleButton = cut.Find($"[data-testid='btn-toggle-status-{optionId}']");
        toggleButton.Click();

        // Assert
        mockService.Verify(x => x.ChangeGameDayOptionStatusAsync(optionId, false, It.IsAny<CancellationToken>()), Times.Once);
    }
}
