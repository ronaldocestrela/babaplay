using System;
using System.Collections.Generic;
using System.Threading;
using Bunit;
using FluentAssertions;
using Moq;
using Xunit;
using BabaPlay.Web.Models;
using BabaPlay.Web.Pages.Settings;
using BabaPlay.Web.Services.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Tests.Web.Settings;

public class PositionsManagementTests : TestContext
{
    [Fact]
    public void PositionsManagement_ShouldRenderPositionsList()
    {
        // Arrange
        var mockService = new Mock<IPositionApiService>();
        var positions = new List<PositionDto>
        {
            new(Guid.NewGuid(), "GOL", "Goleiro", "Guarda-redes", true),
            new(Guid.NewGuid(), "ZAG", "Zagueiro", "Defensor central", true),
            new(Guid.NewGuid(), "ATA", "Atacante", "Artilheiro do time", true)
        };

        mockService
            .Setup(x => x.GetPositionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(positions);

        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<PositionsManagement>();

        // Assert
        cut.Markup.Should().Contain("Goleiro");
        cut.Markup.Should().Contain("Zagueiro");
        cut.Markup.Should().Contain("Atacante");
        cut.Markup.Should().Contain("GOL");
        cut.Markup.Should().Contain("ZAG");
        cut.Markup.Should().Contain("ATA");
    }

    [Fact]
    public void PositionsManagement_WhenDeleteClicked_ShouldCallDeleteService()
    {
        // Arrange
        var mockService = new Mock<IPositionApiService>();
        var positionId = Guid.NewGuid();
        var positions = new List<PositionDto>
        {
            new(positionId, "PIV", "Pivô", "Ponta de lança no futsal", true)
        };

        mockService
            .Setup(x => x.GetPositionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(positions);

        mockService
            .Setup(x => x.DeletePositionAsync(positionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, null));

        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<PositionsManagement>();

        // Act
        var deleteButton = cut.Find("button.btn-delete-position");
        deleteButton.Click();

        // Assert
        mockService.Verify(x => x.DeletePositionAsync(positionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void PositionsManagement_WhenDeleteFailsWithConflict_ShouldShowAlertBanner()
    {
        // Arrange
        var mockService = new Mock<IPositionApiService>();
        var positionId = Guid.NewGuid();
        var positions = new List<PositionDto>
        {
            new(positionId, "MEI", "Meia", "Armador de jogadas", true)
        };

        mockService
            .Setup(x => x.GetPositionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(positions);

        mockService
            .Setup(x => x.DeletePositionAsync(positionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "Esta posição está em uso por um ou mais atletas e não pode ser excluída."));

        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<PositionsManagement>();

        // Act
        var deleteButton = cut.Find("button.btn-delete-position");
        deleteButton.Click();

        // Assert
        cut.Find("[data-testid='alert-banner']").Should().NotBeNull();
        cut.Markup.Should().Contain("Esta posição está em uso por um ou mais atletas");
    }
}
