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
    [Fact]
    public void MatchStatsSummaryPage_ShouldRenderScoreboardAndPlayerTables()
    {
        // Arrange
        var mockMatchService = new Mock<IMatchApiService>();
        var matchId = Guid.NewGuid();

        mockMatchService
            .Setup(x => x.GetMatchesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MatchDto>());

        Services.AddSingleton(mockMatchService.Object);

        // Act
        var cut = RenderComponent<MatchStatsSummary>(parameters => parameters
            .Add(p => p.MatchId, matchId));

        // Assert
        cut.Markup.Should().Contain("Súmula Pós-Jogo & Estatísticas em Tempo Real");
        cut.Find("[data-testid='btn-save-stats']").Should().NotBeNull();
        cut.Find("[data-testid='home-team-table']").Should().NotBeNull();
        cut.Find("[data-testid='away-team-table']").Should().NotBeNull();
    }
}
