using System;
using System.Collections.Generic;
using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Matches;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Matches;

public class MvpLeaderboardWidgetTests : TestContext
{
    [Fact]
    public void MvpLeaderboardWidget_ShouldRenderPodiumWithFirstSecondThirdPlace()
    {
        // Arrange
        var candidates = new List<MvpCandidateDto>
        {
            new(Guid.NewGuid(), "Craque 1º Lugar", null, "Atacante", "Time Amarelo", 3, 1, 10, 50.0, false),
            new(Guid.NewGuid(), "Craque 2º Lugar", null, "Meia", "Time Azul", 1, 2, 6, 30.0, false),
            new(Guid.NewGuid(), "Craque 3º Lugar", null, "Zagueiro", "Time Amarelo", 0, 1, 4, 20.0, false)
        };

        // Act
        var cut = RenderComponent<MvpLeaderboardWidget>(parameters => parameters
            .Add(p => p.Candidates, candidates)
            .Add(p => p.TotalVotes, 20));

        // Assert
        cut.Find("[data-testid='leaderboard-title']").TextContent.Should().Contain("Pódio do Craque do Jogo");
        cut.Find("[data-testid='total-votes']").TextContent.Should().Contain("20 Votos");
        cut.Find("[data-testid='podium-1st']").TextContent.Should().Contain("Craque 1º Lugar");
        cut.Find("[data-testid='podium-2nd']").TextContent.Should().Contain("Craque 2º Lugar");
        cut.Find("[data-testid='podium-3rd']").TextContent.Should().Contain("Craque 3º Lugar");
    }
}
