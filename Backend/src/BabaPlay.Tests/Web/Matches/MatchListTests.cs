using System;
using System.Collections.Generic;
using System.Threading;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using BabaPlay.Web.Models;
using BabaPlay.Web.Pages.Matches;
using BabaPlay.Web.Services.Http;

namespace BabaPlay.Tests.Web.Matches;

public class MatchListTests : TestContext
{
    [Fact]
    public void MatchList_ShouldRenderMatchesList()
    {
        // Arrange
        var mockService = new Mock<IMatchApiService>();
        var gameDays = new List<GameDayDto>
        {
            new(Guid.NewGuid(), "Baba Terça-Feira", DateTime.Now.AddDays(1), "Arena 1", "Jogo semanal", 20, "Confirmed"),
            new(Guid.NewGuid(), "Baba Quinta-Feira", DateTime.Now.AddDays(3), "Arena 2", "Treino", 16, "Confirmed")
        };

        mockService
            .Setup(x => x.GetGameDaysAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gameDays);

        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<MatchList>();

        // Assert
        cut.Markup.Should().Contain("Baba Terça-Feira");
        cut.Markup.Should().Contain("Baba Quinta-Feira");
        cut.Markup.Should().Contain("Arena 1");
        cut.Markup.Should().Contain("Arena 2");
    }

    [Fact]
    public void MatchList_WhenNoMatchesReturned_ShouldShowEmptyState()
    {
        // Arrange
        var mockService = new Mock<IMatchApiService>();
        mockService
            .Setup(x => x.GetGameDaysAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameDayDto>());

        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<MatchList>();

        // Assert
        cut.Find("[data-testid='empty-matches-state']").Should().NotBeNull();
        cut.Markup.Should().Contain("Nenhum baba ou partida encontrada");
    }
}
