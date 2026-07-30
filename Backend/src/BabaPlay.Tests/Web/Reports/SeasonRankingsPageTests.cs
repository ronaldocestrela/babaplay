using System;
using System.Collections.Generic;
using System.Threading;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using BabaPlay.Web.Models;
using BabaPlay.Web.Pages.Reports;
using BabaPlay.Web.Services.Http;

namespace BabaPlay.Tests.Web.Reports;

public class SeasonRankingsPageTests : TestContext
{
    [Fact]
    public void SeasonRankingsPage_ShouldRenderNavigationTabsAndGeneralRankingTable()
    {
        // Arrange
        var mockRankingService = new Mock<IRankingApiService>();

        mockRankingService
            .Setup(x => x.GetRankingAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RankingEntryDto>
            {
                new(Guid.NewGuid(), "Zico", "Galinho", null, 30, 10, 10, 0, 0, 1)
            });

        mockRankingService
            .Setup(x => x.GetTopScorersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TopScorerEntryDto>());

        mockRankingService
            .Setup(x => x.GetAttendanceAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AttendanceEntryDto>());

        Services.AddSingleton(mockRankingService.Object);

        // Act
        var cut = RenderComponent<SeasonRankings>();

        // Assert
        cut.Markup.Should().Contain("Rankings da Temporada & Histórico");
        cut.Find("[data-testid='btn-rebuild-ranking']").Should().NotBeNull();
        cut.Find("[data-testid='ranking-tabs']").Should().NotBeNull();
        cut.Find("[data-testid='general-ranking-table']").Should().NotBeNull();
        cut.Markup.Should().Contain("Zico");
    }
}
