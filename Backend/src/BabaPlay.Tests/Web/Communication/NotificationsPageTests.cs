using System;
using System.Collections.Generic;
using System.Threading;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using BabaPlay.Web.Pages.Communication;
using BabaPlay.Web.Models;
using BabaPlay.Web.Services.Http;

namespace BabaPlay.Tests.Web.Communication;

public class NotificationsPageTests : TestContext
{
    private readonly Mock<INotificationApiService> _mockService = new();

    public NotificationsPageTests()
    {
        this.AddTestAuthorization().SetAuthorized("Test User");
        Services.AddSingleton(_mockService.Object);
    }

    [Fact]
    public void NotificationsPage_ShouldRenderNotificationsList()
    {
        // Arrange
        var summary = new NotificationSummaryDto(
            UnreadCount: 1,
            Notifications:
            [
                new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, "Aviso de Jogo", "Mensagem de teste 1", null, false, null, DateTime.UtcNow),
                new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 4, "Fatura Gerada", "Mensagem de teste 2", null, true, DateTime.UtcNow, DateTime.UtcNow.AddDays(-1))
            ]
        );

        _mockService
            .Setup(x => x.GetNotificationsAsync(false, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        // Act
        var cut = RenderComponent<Notifications>();

        // Assert
        cut.FindAll("[data-testid='notification-card-item']").Should().HaveCount(2);
        cut.Find("[data-testid='unread-badge']").TextContent.Should().Contain("1 não lida");
    }

    [Fact]
    public void NotificationsPage_WhenEmpty_ShouldRenderEmptyState()
    {
        // Arrange
        var summary = new NotificationSummaryDto(UnreadCount: 0, Notifications: []);

        _mockService
            .Setup(x => x.GetNotificationsAsync(false, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        // Act
        var cut = RenderComponent<Notifications>();

        // Assert
        cut.Find("[data-testid='empty-notifications-page']").TextContent.Should().Contain("Nenhuma notificação encontrada");
    }
}
