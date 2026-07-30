using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Moq;
using Xunit;
using BabaPlay.Web.Models;
using BabaPlay.Web.Pages.Players;
using BabaPlay.Web.Services.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Tests.Web.Players;

public class CompleteProfileTests : TestContext
{
    [Fact]
    public void CompleteProfile_ShouldRenderFormWithPositionsLoaded()
    {
        // Arrange
        var mockPlayerService = new Mock<IPlayerApiService>();
        var positions = new List<PositionDto>
        {
            new(Guid.NewGuid(), "Atacante", "ATA"),
            new(Guid.NewGuid(), "Zagueiro", "ZAG")
        };

        mockPlayerService
            .Setup(x => x.GetPositionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(positions);

        Services.AddSingleton(mockPlayerService.Object);

        // Act
        var cut = RenderComponent<CompleteProfile>();

        // Assert
        cut.Find("#player-name").Should().NotBeNull();
        cut.Find("#player-nickname").Should().NotBeNull();
        cut.Find("#preferred-foot").Should().NotBeNull();
        cut.Find("#primary-position").Should().NotBeNull();
        cut.Find("#complete-profile-submit").Should().NotBeNull();
        cut.Markup.Should().Contain("Atacante (ATA)");
    }
}
