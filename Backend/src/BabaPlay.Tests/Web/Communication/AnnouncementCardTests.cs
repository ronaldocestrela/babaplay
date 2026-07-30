using System;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using Xunit;
using BabaPlay.Web.Components.Communication;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Communication;

public class AnnouncementCardTests : TestContext
{
    [Fact]
    public void AnnouncementCard_WithIsNewAndUnread_ShouldRenderBadgesAndTitle()
    {
        // Arrange
        var item = new AnnouncementDto(
            Id: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            AuthorId: Guid.NewGuid(),
            AuthorName: "Diretoria",
            Title: "Aviso de Teste",
            Content: "Conteúdo do aviso para associados.",
            IsPublished: true,
            PublishedAtUtc: DateTime.UtcNow,
            ExpiresAtUtc: null,
            IsRead: false,
            IsNew: true,
            Tags: "importante, regras",
            CreatedAt: DateTime.UtcNow
        );

        // Act
        var cut = RenderComponent<AnnouncementCard>(parameters => parameters
            .Add(p => p.Announcement, item));

        // Assert
        cut.Find("[data-testid='announcement-title']").TextContent.Should().Contain("Aviso de Teste");
        cut.Find("[data-testid='announcement-author']").TextContent.Should().Contain("Diretoria");
        cut.Find("[data-testid='badge-new']").TextContent.Should().Contain("Novo");
        cut.Find("[data-testid='badge-unread']").TextContent.Should().Contain("Não lido");
        cut.Find("[data-testid='btn-mark-read']").Should().NotBeNull();
    }

    [Fact]
    public void AnnouncementCard_WhenRead_ShouldNotRenderMarkReadButton()
    {
        // Arrange
        var item = new AnnouncementDto(
            Id: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            AuthorId: Guid.NewGuid(),
            AuthorName: "Diretoria",
            Title: "Aviso Lido",
            Content: "Conteúdo já lido.",
            IsPublished: true,
            PublishedAtUtc: DateTime.UtcNow.AddDays(-2),
            ExpiresAtUtc: null,
            IsRead: true,
            IsNew: false,
            Tags: null,
            CreatedAt: DateTime.UtcNow.AddDays(-2)
        );

        // Act
        var cut = RenderComponent<AnnouncementCard>(parameters => parameters
            .Add(p => p.Announcement, item));

        // Assert
        cut.Find("[data-testid='badge-read']").TextContent.Should().Contain("Lido");
        cut.FindAll("[data-testid='btn-mark-read']").Should().BeEmpty();
    }

    [Fact]
    public async Task AnnouncementCard_ClickMarkRead_ShouldTriggerOnReadCallback()
    {
        // Arrange
        var item = new AnnouncementDto(
            Id: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            AuthorId: Guid.NewGuid(),
            AuthorName: "Diretoria",
            Title: "Aviso Pendente",
            Content: "Conteúdo pendente.",
            IsPublished: true,
            PublishedAtUtc: DateTime.UtcNow,
            ExpiresAtUtc: null,
            IsRead: false,
            IsNew: true,
            Tags: null,
            CreatedAt: DateTime.UtcNow
        );

        var callbackCalled = false;

        // Act
        var cut = RenderComponent<AnnouncementCard>(parameters => parameters
            .Add(p => p.Announcement, item)
            .Add(p => p.OnRead, () => callbackCalled = true));

        var btn = cut.Find("[data-testid='btn-mark-read']");
        await btn.ClickAsync(new MouseEventArgs());

        // Assert
        callbackCalled.Should().BeTrue();
    }
}
