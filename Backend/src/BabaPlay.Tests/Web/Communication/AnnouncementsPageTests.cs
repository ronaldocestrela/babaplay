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

public class AnnouncementsPageTests : TestContext
{
    private readonly Mock<ICommunicationApiService> _mockService = new();

    public AnnouncementsPageTests()
    {
        Services.AddSingleton(_mockService.Object);
    }

    [Fact]
    public void AnnouncementsPage_ShouldRenderCardsList()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("Admin User");
        authContext.SetPolicies("CommunicationWrite");

        var list = new List<AnnouncementDto>
        {
            new(
                Id: Guid.NewGuid(),
                TenantId: Guid.NewGuid(),
                AuthorId: Guid.NewGuid(),
                AuthorName: "Diretoria",
                Title: "Aviso 1",
                Content: "Conteúdo 1",
                IsPublished: true,
                PublishedAtUtc: DateTime.UtcNow,
                ExpiresAtUtc: null,
                IsRead: false,
                IsNew: true,
                Tags: null,
                CreatedAt: DateTime.UtcNow
            ),
            new(
                Id: Guid.NewGuid(),
                TenantId: Guid.NewGuid(),
                AuthorId: Guid.NewGuid(),
                AuthorName: "Diretoria",
                Title: "Aviso 2",
                Content: "Conteúdo 2",
                IsPublished: true,
                PublishedAtUtc: DateTime.UtcNow.AddDays(-1),
                ExpiresAtUtc: null,
                IsRead: true,
                IsNew: false,
                Tags: null,
                CreatedAt: DateTime.UtcNow.AddDays(-1)
            )
        };

        _mockService
            .Setup(x => x.GetAnnouncementsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Act
        var cut = RenderComponent<Announcements>();

        // Assert
        cut.FindAll("[data-testid='announcement-card']").Should().HaveCount(2);
        cut.Find("[data-testid='unread-badge']").TextContent.Should().Contain("1 não lido");
    }

    [Fact]
    public void AnnouncementsPage_WhenEmpty_ShouldRenderEmptyState()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("Atleta User");

        _mockService
            .Setup(x => x.GetAnnouncementsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AnnouncementDto>());

        // Act
        var cut = RenderComponent<Announcements>();

        // Assert
        cut.Find("[data-testid='empty-announcements']").TextContent.Should().Contain("Nenhum comunicado por aqui");
    }

    [Fact]
    public void AnnouncementsPage_WhenApiReturnsNull_ShouldShowErrorAlert()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("Member");

        _mockService
            .Setup(x => x.GetAnnouncementsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<AnnouncementDto>?)null);

        // Act
        var cut = RenderComponent<Announcements>();

        // Assert
        cut.Find("[data-testid='announcements-error-alert']").TextContent.Should().Contain("Não foi possível carregar os comunicados");
    }
}
