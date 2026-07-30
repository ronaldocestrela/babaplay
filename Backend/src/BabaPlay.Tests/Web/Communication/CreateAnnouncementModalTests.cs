using System;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using BabaPlay.Web.Components.Communication;
using BabaPlay.Web.Models;
using BabaPlay.Web.Services.Http;

namespace BabaPlay.Tests.Web.Communication;

public class CreateAnnouncementModalTests : TestContext
{
    private readonly Mock<ICommunicationApiService> _mockService = new();

    public CreateAnnouncementModalTests()
    {
        Services.AddSingleton(_mockService.Object);
    }

    [Fact]
    public void CreateAnnouncementModal_WhenNotOpen_ShouldRenderNothing()
    {
        // Act
        var cut = RenderComponent<CreateAnnouncementModal>(parameters => parameters
            .Add(p => p.IsOpen, false));

        // Assert
        cut.FindAll("[data-testid='create-announcement-modal']").Should().BeEmpty();
    }

    [Fact]
    public void CreateAnnouncementModal_WhenOpen_ShouldRenderForm()
    {
        // Act
        var cut = RenderComponent<CreateAnnouncementModal>(parameters => parameters
            .Add(p => p.IsOpen, true));

        // Assert
        cut.Find("[data-testid='create-announcement-modal']").Should().NotBeNull();
        cut.Find("[data-testid='input-announcement-title']").Should().NotBeNull();
        cut.Find("[data-testid='input-announcement-content']").Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAnnouncementModal_SubmitValidForm_ShouldCallApiAndTriggerOnCreated()
    {
        // Arrange
        AnnouncementDto? createdDto = null;
        var mockResponse = new AnnouncementDto(
            Id: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            AuthorId: Guid.NewGuid(),
            AuthorName: "Admin",
            Title: "Título Criado",
            Content: "Conteúdo do modal",
            IsPublished: true,
            PublishedAtUtc: DateTime.UtcNow,
            ExpiresAtUtc: null,
            IsRead: false,
            IsNew: true,
            Tags: "teste",
            CreatedAt: DateTime.UtcNow
        );

        _mockService
            .Setup(x => x.CreateAnnouncementAsync(It.IsAny<CreateAnnouncementDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        var cut = RenderComponent<CreateAnnouncementModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnCreated, (AnnouncementDto dto) => createdDto = dto));

        // Fill form fields
        cut.Find("[data-testid='input-announcement-title']").Change("Título Criado");
        cut.Find("[data-testid='input-announcement-content']").Change("Conteúdo do modal");

        // Submit form
        await cut.Find("[data-testid='form-create-announcement']").SubmitAsync();

        // Assert
        _mockService.Verify(x => x.CreateAnnouncementAsync(It.IsAny<CreateAnnouncementDto>(), It.IsAny<CancellationToken>()), Times.Once);
        createdDto.Should().NotBeNull();
        createdDto!.Title.Should().Be("Título Criado");
    }
}
