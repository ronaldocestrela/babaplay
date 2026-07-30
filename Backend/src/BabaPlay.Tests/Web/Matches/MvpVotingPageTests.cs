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

public class MvpVotingPageTests : TestContext
{
    [Fact]
    public void MvpVotingPage_ShouldRenderVotingGridAndLeaderboard()
    {
        // Arrange
        var mockMatchService = new Mock<IMatchApiService>();
        var matchId = Guid.NewGuid();

        var candidates = new List<MvpCandidateDto>
        {
            new(Guid.NewGuid(), "Zico", null, "Meia", "Time Amarelo", 2, 1, 5, 50.0, false)
        };

        var resultData = new MvpResultDto(matchId, 10, true, null, "Zico", candidates);

        mockMatchService
            .Setup(x => x.GetMvpResultsAsync(matchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultData);

        Services.AddSingleton(mockMatchService.Object);

        // Act
        var cut = RenderComponent<MvpVoting>(parameters => parameters
            .Add(p => p.MatchId, matchId));

        // Assert
        cut.Markup.Should().Contain("Votação do Craque do Jogo (MVP)");
        cut.Find("[data-testid='btn-refresh-mvp']").Should().NotBeNull();
        cut.Find("[data-testid='mvp-leaderboard-widget']").Should().NotBeNull();
        cut.Find("[data-testid='candidates-grid']").Should().NotBeNull();
    }
}
