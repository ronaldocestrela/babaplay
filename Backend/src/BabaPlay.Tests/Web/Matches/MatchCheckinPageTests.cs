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

public class MatchCheckinPageTests : TestContext
{
    [Fact]
    public void MatchCheckinPage_ShouldRenderRsvpWidgets()
    {
        // Arrange
        var mockCheckinService = new Mock<ICheckinApiService>();
        var mockMatchService = new Mock<IMatchApiService>();

        var gameDayId = Guid.NewGuid();
        var rsvpSummary = new GameDayRsvpSummaryDto(
            gameDayId,
            "Baba de Quinta-Feira",
            DateTime.Now.AddDays(1),
            "Arena 1",
            20,
            12,
            0,
            "Confirmed",
            null
        );

        mockCheckinService
            .Setup(x => x.GetGameDayRsvpSummaryAsync(gameDayId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rsvpSummary);

        mockCheckinService
            .Setup(x => x.GetRsvpListByGameDayAsync(gameDayId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerRsvpDetailDto>());

        Services.AddSingleton(mockCheckinService.Object);
        Services.AddSingleton(mockMatchService.Object);

        // Act
        var cut = RenderComponent<MatchCheckin>(parameters => parameters
            .Add(p => p.GameDayId, gameDayId));

        // Assert
        cut.Markup.Should().Contain("Confirmação de Presença & Check-in");
        cut.Markup.Should().Contain("Baba de Quinta-Feira");
        cut.Find("[data-testid='btn-geo-checkin']").Should().NotBeNull();
    }
}
