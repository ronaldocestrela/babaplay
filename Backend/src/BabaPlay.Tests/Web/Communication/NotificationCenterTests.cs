using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using BabaPlay.Web.Components.Communication;
using BabaPlay.Web.Models;
using BabaPlay.Web.Services.Http;

namespace BabaPlay.Tests.Web.Communication;

public class NotificationCenterTests : TestContext
{
    private readonly Mock<INotificationApiService> _mockService = new();

    public NotificationCenterTests()
    {
        this.AddTestAuthorization().SetAuthorized("Test User");
        Services.AddSingleton(_mockService.Object);
    }

    [Fact]
    public void NotificationCenter_WhenUnreadCountGtZero_ShouldRenderBadge()
    {
        // Arrange
        var summary = new NotificationSummaryDto(
            UnreadCount: 3,
            Notifications:
            [
                new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, "Jogo Sábado", "Não se esqueça do jogo", null, false, null, DateTime.UtcNow)
            ]
        );

        _mockService
            .Setup(x => x.GetNotificationsAsync(false, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        // Act
        var cut = RenderComponent<NotificationCenter>();

        // Assert
        cut.Find("[data-testid='unread-count-badge']").TextContent.Should().Be("3");
    }

    [Fact]
    public async Task NotificationCenter_ClickBell_ShouldOpenDropdown()
    {
        // Arrange
        var summary = new NotificationSummaryDto(
            UnreadCount: 1,
            Notifications:
            [
                new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 2, "Checkin Realizado", "Você confirmou presença", null, false, null, DateTime.UtcNow)
            ]
        );

        _mockService
            .Setup(x => x.GetNotificationsAsync(false, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        var cut = RenderComponent<NotificationCenter>();

        // Act
        var bellBtn = cut.Find("[data-testid='btn-notification-bell']");
        await bellBtn.ClickAsync(new MouseEventArgs());

        // Assert
        cut.Find("[data-testid='notification-dropdown']").Should().NotBeNull();
        cut.Find("[data-testid='notification-item']").TextContent.Should().Contain("Checkin Realizado");
    }
}
