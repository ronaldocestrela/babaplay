using System;
using System.Collections.Generic;
using System.Threading;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using BabaPlay.Web.Models;
using BabaPlay.Web.Pages.Teams;
using BabaPlay.Web.Services.Http;

namespace BabaPlay.Tests.Web.Teams;

public class TeamDrawPageTests : TestContext
{
    [Fact]
    public void TeamDrawPage_ShouldRenderTeamsGridAndAllowMovingPlayersBetweenColumns()
    {
        // Arrange
        var mockTeamService = new Mock<ITeamApiService>();
        var mockMatchService = new Mock<IMatchApiService>();

        var gameDayId = Guid.NewGuid();
        var team1Id = Guid.NewGuid();
        var team2Id = Guid.NewGuid();

        var player1 = new DrawnPlayerDto(Guid.NewGuid(), "Craque A", null, "Meia", 5, null, team1Id);
        var player2 = new DrawnPlayerDto(Guid.NewGuid(), "Craque B", null, "Atacante", 3, null, team2Id);

        var teams = new List<DrawnTeamDto>
        {
            new(team1Id, "Time Amarelo 🟡", "#f59e0b", "bg-warning text-dark", new List<DrawnPlayerDto> { player1 }, 5.0),
            new(team2Id, "Time Azul 🔵", "#3b82f6", "bg-primary text-white", new List<DrawnPlayerDto> { player2 }, 3.0)
        };

        var drawResult = new DrawResultDto(gameDayId, teams, 2);

        mockTeamService
            .Setup(x => x.GenerateBalancedTeamsAsync(It.IsAny<GenerateBalancedTeamsDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(drawResult);

        Services.AddSingleton(mockTeamService.Object);
        Services.AddSingleton(mockMatchService.Object);

        // Act
        var cut = RenderComponent<TeamDraw>(parameters => parameters
            .Add(p => p.GameDayId, gameDayId));

        // Assert - Initial render
        cut.Markup.Should().Contain("Sorteio de Times & Coletes");
        cut.Markup.Should().Contain("Time Amarelo");
        cut.Markup.Should().Contain("Time Azul");
        cut.Find("[data-testid='btn-draw-teams']").Should().NotBeNull();

        // Move player1 from Team 1 to Team 2
        var moveRightBtn = cut.Find("[data-testid='btn-move-right']");
        moveRightBtn.Click();

        // Verify player moved and star averages recalculated
        cut.Find($"[data-testid='team-column-{team1Id}'] [data-testid='team-count']").TextContent.Should().Contain("0 atletas");
        cut.Find($"[data-testid='team-column-{team2Id}'] [data-testid='team-count']").TextContent.Should().Contain("2 atletas");
    }
}
