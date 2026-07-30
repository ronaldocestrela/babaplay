using System;
using System.Collections.Generic;
using System.Threading;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using BabaPlay.Web.Components.Financial;
using BabaPlay.Web.Models;
using BabaPlay.Web.Services.Http;

namespace BabaPlay.Tests.Web.Financial;

public class CreateInvoiceModalTests : TestContext
{
    [Fact]
    public void CreateInvoiceModal_WhenNotOpen_ShouldRenderNothing()
    {
        // Arrange
        var mockService = new Mock<IFinancialApiService>();
        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<CreateInvoiceModal>(parameters => parameters
            .Add(p => p.IsOpen, false));

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void CreateInvoiceModal_WhenOpen_ShouldRenderFieldsAndPlayers()
    {
        // Arrange
        var mockService = new Mock<IFinancialApiService>();
        Services.AddSingleton(mockService.Object);

        var players = new List<PlayerDto>
        {
            new PlayerDto(Guid.NewGuid(), Guid.NewGuid(), "Carlos Atleta", "Carlinhos", null, null, "Right", null, "Atacante", 10, null, null, null, true, DateTime.UtcNow)
        };

        // Act
        var cut = RenderComponent<CreateInvoiceModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Players, players));

        // Assert
        cut.Markup.Should().Contain("Emitir Nova Cobrança");
        cut.Markup.Should().Contain("Carlos Atleta (Carlinhos)");
        cut.Find("[data-testid='input-amount']").Should().NotBeNull();
    }

    [Fact]
    public async Task CreateInvoiceModal_Submit_WhenValid_ShouldCallApiService()
    {
        // Arrange
        var mockService = new Mock<IFinancialApiService>();
        var createdDto = new InvoiceDto
        {
            Id = Guid.NewGuid(),
            Amount = 50m,
            Status = 0
        };

        mockService
            .Setup(x => x.CreateInvoiceAsync(It.IsAny<CreateInvoiceDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdDto);

        Services.AddSingleton(mockService.Object);

        var playerId = Guid.NewGuid();
        var players = new List<PlayerDto>
        {
            new PlayerDto(playerId, Guid.NewGuid(), "Atleta Teste", "Testinho", null, null, "Right", null, "Meia", 8, null, null, null, true, DateTime.UtcNow)
        };

        bool eventFired = false;

        // Act
        var cut = RenderComponent<CreateInvoiceModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Players, players)
            .Add(p => p.OnInvoiceCreated, () => eventFired = true));

        // Select player
        var selectPlayer = cut.Find("[data-testid='select-player']");
        selectPlayer.Change(playerId.ToString());

        // Submit form
        var form = cut.Find("form");
        await form.SubmitAsync();

        // Assert
        mockService.Verify(x => x.CreateInvoiceAsync(It.IsAny<CreateInvoiceDto>(), It.IsAny<CancellationToken>()), Times.Once);
        eventFired.Should().BeTrue();
    }
}
