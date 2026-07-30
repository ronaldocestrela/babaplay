using System;
using System.Collections.Generic;
using System.Threading;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using BabaPlay.Web.Models;
using BabaPlay.Web.Pages.Financial;
using BabaPlay.Web.Services.Http;

namespace BabaPlay.Tests.Web.Financial;

public class InvoicesListPageTests : TestContext
{
    [Fact]
    public void InvoicesListPage_ShouldRenderInvoicesTable()
    {
        // Arrange
        var mockFinancialService = new Mock<IFinancialApiService>();
        var mockPlayerService = new Mock<IPlayerApiService>();

        var playerId = Guid.NewGuid();
        var players = new List<PlayerDto>
        {
            new PlayerDto(playerId, Guid.NewGuid(), "Lucas Silva", "Luquinhas", null, null, "Right", null, "Zagueiro", 4, null, null, null, true, DateTime.UtcNow)
        };

        var invoices = new List<InvoiceDto>
        {
            new InvoiceDto
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                PlayerId = playerId,
                Year = 2026,
                Month = 5,
                Amount = 100m,
                PaidAmount = 0m,
                OpenAmount = 100m,
                DueDateUtc = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
                Status = 0,
                Description = "Mensalidade de Maio"
            }
        };

        mockPlayerService
            .Setup(x => x.GetPlayersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(players);

        mockFinancialService
            .Setup(x => x.GetInvoicesAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<Guid?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoices);

        Services.AddSingleton(mockFinancialService.Object);
        Services.AddSingleton(mockPlayerService.Object);

        // Act
        var cut = RenderComponent<InvoicesList>();

        // Assert
        cut.Markup.Should().Contain("Faturas & Mensalidades");
        cut.Markup.Should().Contain("Lucas Silva");
        cut.Markup.Should().Contain("05/2026");
        cut.Markup.Should().Contain("100,00");
        cut.Markup.Should().Contain("Pendente");
    }

    [Fact]
    public void InvoicesListPage_WhenNoInvoices_ShouldShowEmptyState()
    {
        // Arrange
        var mockFinancialService = new Mock<IFinancialApiService>();
        var mockPlayerService = new Mock<IPlayerApiService>();

        mockPlayerService
            .Setup(x => x.GetPlayersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerDto>());

        mockFinancialService
            .Setup(x => x.GetInvoicesAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<Guid?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InvoiceDto>());

        Services.AddSingleton(mockFinancialService.Object);
        Services.AddSingleton(mockPlayerService.Object);

        // Act
        var cut = RenderComponent<InvoicesList>();

        // Assert
        cut.Find("[data-testid='invoices-empty-state']").Should().NotBeNull();
        cut.Markup.Should().Contain("Nenhuma fatura encontrada");
    }
}
