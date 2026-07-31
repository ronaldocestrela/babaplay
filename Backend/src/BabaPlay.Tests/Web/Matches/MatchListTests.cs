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
    private static Mock<ICheckinApiService> CreateCheckinMock(IReadOnlyList<CheckinDto>? checkins = null)
    {
        var mock = new Mock<ICheckinApiService>();
        mock
            .Setup(x => x.GetCheckinsByGameDayAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkins ?? new List<CheckinDto>());
        return mock;
    }

    [Fact]
    public void MatchList_ShouldRenderMatchesList()
    {
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
        Services.AddSingleton(CreateCheckinMock().Object);

        var cut = RenderComponent<MatchList>();

        cut.Markup.Should().Contain("Baba Terça-Feira");
        cut.Markup.Should().Contain("Baba Quinta-Feira");
        cut.Markup.Should().Contain("Arena 1");
        cut.Markup.Should().Contain("Arena 2");
    }

    [Fact]
    public void MatchList_WhenNoMatchesReturned_ShouldShowEmptyState()
    {
        var mockService = new Mock<IMatchApiService>();
        mockService
            .Setup(x => x.GetGameDaysAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameDayDto>());

        Services.AddSingleton(mockService.Object);
        Services.AddSingleton(CreateCheckinMock().Object);

        var cut = RenderComponent<MatchList>();

        cut.Find("[data-testid='empty-matches-state']").Should().NotBeNull();
        cut.Markup.Should().Contain("Nenhum baba ou partida encontrada");
    }

    [Fact]
    public void MatchList_WhenNoCheckins_ShouldShowZeroConfirmedCount()
    {
        var gameDayId = Guid.NewGuid();
        var mockService = new Mock<IMatchApiService>();
        mockService
            .Setup(x => x.GetGameDaysAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameDayDto>
            {
                new(gameDayId, "Baba Novo", DateTime.Now.AddDays(1), "Campo 1", null, 20, "Pending")
            });

        Services.AddSingleton(mockService.Object);
        Services.AddSingleton(CreateCheckinMock().Object);

        var cut = RenderComponent<MatchList>();

        cut.Find(".vagas-count").TextContent.Should().Contain("0");
        cut.Find(".vagas-count").TextContent.Should().Contain("20 vagas");
    }

    [Fact]
    public void MatchList_ShouldShowConfirmedCountFromCheckins()
    {
        var gameDayId = Guid.NewGuid();
        var mockService = new Mock<IMatchApiService>();
        mockService
            .Setup(x => x.GetGameDaysAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameDayDto>
            {
                new(gameDayId, "Baba Confirmados", DateTime.Now.AddDays(1), "Campo 1", null, 20, "Confirmed")
            });

        var checkins = new List<CheckinDto>
        {
            CreateCheckin(gameDayId),
            CreateCheckin(gameDayId),
            CreateCheckin(gameDayId),
        };

        Services.AddSingleton(mockService.Object);
        Services.AddSingleton(CreateCheckinMock(checkins).Object);

        var cut = RenderComponent<MatchList>();

        cut.Find(".vagas-count").TextContent.Should().Contain("3");
        cut.Find(".vagas-count").TextContent.Should().Contain("20 vagas");
    }

    [Fact]
    public void MatchList_WhenStatusButtonClickedOnPending_ShouldCallChangeGameDayStatusAsync()
    {
        var gameDayId = Guid.NewGuid();
        var mockService = new Mock<IMatchApiService>();
        mockService
            .Setup(x => x.GetGameDaysAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameDayDto>
            {
                new(gameDayId, "Baba Pendente", DateTime.Now.AddDays(1), "Campo 1", null, 20, "Pending")
            });

        mockService
            .Setup(x => x.ChangeGameDayStatusAsync(gameDayId, "Confirmed", It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, (string?)null));

        Services.AddSingleton(mockService.Object);
        Services.AddSingleton(CreateCheckinMock().Object);

        var cut = RenderComponent<MatchList>();

        cut.Find("[data-testid='btn-status-match']").Click();

        mockService.Verify(
            x => x.ChangeGameDayStatusAsync(gameDayId, "Confirmed", It.IsAny<CancellationToken>()),
            Times.Once);
        mockService.Verify(
            x => x.ChangeMatchStatusAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static CheckinDto CreateCheckin(Guid gameDayId)
        => new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Atleta Teste",
            null,
            "Atacante",
            gameDayId,
            DateTime.UtcNow,
            -12.9714,
            -38.5014,
            0,
            true,
            "Confirmed");
}
