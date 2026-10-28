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

public class MatchStatsSummaryPageTests : TestContext
{
    private readonly Mock<IMatchApiService> _mockMatchService = new();
    private readonly Mock<ITeamApiService> _mockTeamService = new();
    private readonly Mock<ICheckinApiService> _mockCheckinService = new();
    private readonly Mock<IPlayerApiService> _mockPlayerService = new();

    public MatchStatsSummaryPageTests()
    {
        Services.AddSingleton(_mockMatchService.Object);
        Services.AddSingleton(_mockTeamService.Object);
        Services.AddSingleton(_mockCheckinService.Object);
        Services.AddSingleton(_mockPlayerService.Object);
    }

    [Fact]
    public void MatchStatsSummaryPage_ShouldRenderScoreboardAndPlayerTables()
    {
        // Arrange
        var matchId = Guid.NewGuid();

        _mockMatchService
            .Setup(x => x.GetMatchesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MatchDto>());

        _mockPlayerService
            .Setup(x => x.GetPlayersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerDto>());

        // Act
        var cut = RenderComponent<MatchStatsSummary>(parameters => parameters
            .Add(p => p.MatchId, matchId));

        // Assert
        cut.Markup.Should().Contain("Súmula Pós-Jogo & Estatísticas em Tempo Real");
        cut.Find("[data-testid='btn-save-stats']").Should().NotBeNull();
        cut.Find("[data-testid='home-team-table']").Should().NotBeNull();
        cut.Find("[data-testid='away-team-table']").Should().NotBeNull();
    }

    [Fact]
    public void MatchStatsSummaryPage_WhenActivePlayersExist_ShouldAllocateThemToTeams()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();

        var players = new List<PlayerDto>
        {
            CreateTestPlayer(player1Id, "Carlos Silva", "Carlinhos"),
            CreateTestPlayer(player2Id, "Eduardo Ramos", "Dudu")
        };

        _mockPlayerService
            .Setup(x => x.GetPlayersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(players);

        // Act
        var cut = RenderComponent<MatchStatsSummary>(parameters => parameters
            .Add(p => p.MatchId, matchId));

        // Assert
        cut.Markup.Should().Contain("Carlos Silva");
        cut.Markup.Should().Contain("Eduardo Ramos");
        cut.Find($"[data-testid='player-stats-row-{player1Id}']").Should().NotBeNull();
        cut.Find($"[data-testid='player-stats-row-{player2Id}']").Should().NotBeNull();
    }

    [Fact]
    public void MatchStatsSummaryPage_WhenSaveClicked_ShouldCallRegisterMatchStatsAsync()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var playerId = Guid.NewGuid();

        var players = new List<PlayerDto>
        {
            CreateTestPlayer(playerId, "Carlos Silva", "Carlinhos")
        };

        _mockPlayerService
            .Setup(x => x.GetPlayersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(players);

        _mockMatchService
            .Setup(x => x.RegisterMatchStatsAsync(It.IsAny<Guid>(), It.IsAny<RegisterMatchStatsDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, null));

        var cut = RenderComponent<MatchStatsSummary>(parameters => parameters
            .Add(p => p.MatchId, matchId));

        // Act
        var saveBtn = cut.Find("[data-testid='btn-save-stats']");
        saveBtn.Click();

        // Assert
        _mockMatchService.Verify(x => x.RegisterMatchStatsAsync(
            It.IsAny<Guid>(),
            It.Is<RegisterMatchStatsDto>(d => d.PlayerStats.Count == 1 && d.PlayerStats[0].PlayerId == playerId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static PlayerDto CreateTestPlayer(Guid id, string name, string nickname) => new(
        id,
        Guid.NewGuid(),
        name,
        nickname,
        "71999990000",
        null,
        "Right",
        null,
        "Atacante",
        10,
        null,
        null,
        "Atleta",
        true,
        DateTime.UtcNow);
}
