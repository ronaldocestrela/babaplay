using System;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using BabaPlay.Web.Pages.Teams;
using BabaPlay.Web.Services.Http;

namespace BabaPlay.Tests.Web.Teams;

public class TacticalBoardPageTests : TestContext
{
    [Fact]
    public void TacticalBoardPage_ShouldRenderTacticalBoardPageAndControls()
    {
        // Arrange
        var mockTeamService = new Mock<ITeamApiService>();
        var mockMatchService = new Mock<IMatchApiService>();

        Services.AddSingleton(mockTeamService.Object);
        Services.AddSingleton(mockMatchService.Object);

        // Act
        var cut = RenderComponent<TacticalBoardPage>();

        // Assert
        cut.Markup.Should().Contain("Prancheta Tática & Escalação Visual");
        cut.Find("[data-testid='select-team']").Should().NotBeNull();
        cut.Find("[data-testid='select-preset']").Should().NotBeNull();
        cut.Find("[data-testid='btn-reset-formation']").Should().NotBeNull();
    }
}
