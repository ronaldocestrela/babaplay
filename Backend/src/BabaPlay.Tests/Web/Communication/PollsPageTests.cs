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

public class PollsPageTests : TestContext
{
    private readonly Mock<ICommunicationApiService> _mockService = new();

    public PollsPageTests()
    {
        Services.AddSingleton(_mockService.Object);
    }

    [Fact]
    public void PollsPage_ShouldRenderCardsList()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("Admin User");
        authContext.SetPolicies("CommunicationWrite");

        var list = new List<PollDto>
        {
            new(
                Id: Guid.NewGuid(),
                TenantId: Guid.NewGuid(),
                AuthorId: Guid.NewGuid(),
                AuthorName: "Diretoria",
                Title: "Enquete 1",
                Description: null,
                ExpiresAtUtc: null,
                IsClosed: false,
                HasVoted: false,
                VotedOptionId: null,
                TotalVotes: 0,
                Options: [new(Guid.NewGuid(), "Opção A", 1, 0, 0.0), new(Guid.NewGuid(), "Opção B", 2, 0, 0.0)],
                CreatedAt: DateTime.UtcNow
            ),
            new(
                Id: Guid.NewGuid(),
                TenantId: Guid.NewGuid(),
                AuthorId: Guid.NewGuid(),
                AuthorName: "Diretoria",
                Title: "Enquete 2",
                Description: null,
                ExpiresAtUtc: null,
                IsClosed: true,
                HasVoted: true,
                VotedOptionId: null,
                TotalVotes: 5,
                Options: [new(Guid.NewGuid(), "Opção 1", 1, 5, 100.0)],
                CreatedAt: DateTime.UtcNow.AddDays(-2)
            )
        };

        _mockService
            .Setup(x => x.GetPollsAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Act
        var cut = RenderComponent<Polls>();

        // Assert
        cut.FindAll("[data-testid='poll-card']").Should().HaveCount(2);
        cut.Find("[data-testid='pending-badge']").TextContent.Should().Contain("1 pendente");
    }

    [Fact]
    public void PollsPage_WhenEmpty_ShouldRenderEmptyState()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("Member");

        _mockService
            .Setup(x => x.GetPollsAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var cut = RenderComponent<Polls>();

        // Assert
        cut.Find("[data-testid='empty-polls']").TextContent.Should().Contain("Nenhuma enquete por aqui");
    }

    [Fact]
    public void PollsPage_WhenApiReturnsNull_ShouldShowErrorAlert()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("Member");

        _mockService
            .Setup(x => x.GetPollsAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<PollDto>?)null);

        // Act
        var cut = RenderComponent<Polls>();

        // Assert
        cut.Find("[data-testid='polls-error-alert']").TextContent.Should().Contain("Não foi possível carregar as enquetes");
    }
}
