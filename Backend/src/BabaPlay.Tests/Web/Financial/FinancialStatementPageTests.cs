using System;
using System.Collections.Generic;
using System.Threading;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Xunit;
using BabaPlay.Web.Pages.Financial;
using BabaPlay.Web.Models;
using BabaPlay.Web.Services.Http;

namespace BabaPlay.Tests.Web.Financial;

public class FinancialStatementPageTests : TestContext
{
    [Fact]
    public void FinancialStatementPage_ShouldRenderBalanceteAndTransactionsTable()
    {
        // Arrange
        var mockService = new Mock<IFinancialApiService>();
        var dto = new FinancialStatementDto
        {
            PeriodYear = 2026,
            PeriodMonth = 5,
            TotalIncomes = 1200m,
            TotalExpenses = 500m,
            NetBalance = 700m,
            PublishedAtUtc = DateTime.UtcNow,
            Items = new List<FinancialStatementItemDto>
            {
                new FinancialStatementItemDto
                {
                    TransactionId = Guid.NewGuid(),
                    OccurredOnUtc = DateTime.UtcNow,
                    Description = "Mensalidade Maio",
                    Type = 0, // Income
                    Amount = 100m,
                    Category = "Mensalidade",
                    PlayerName = "Carlos Silva"
                },
                new FinancialStatementItemDto
                {
                    TransactionId = Guid.NewGuid(),
                    OccurredOnUtc = DateTime.UtcNow,
                    Description = "Aluguel de Campo",
                    Type = 1, // Expense
                    Amount = 250m,
                    Category = "Aluguel de Campo",
                    PlayerName = null
                }
            }
        };

        mockService
            .Setup(x => x.GetFinancialStatementAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        Services.AddSingleton(mockService.Object);
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = RenderComponent<FinancialStatement>();

        // Assert
        cut.Find("[data-testid='val-total-incomes']").TextContent.Should().Contain("1.200,00");
        cut.Find("[data-testid='val-total-expenses']").TextContent.Should().Contain("500,00");
        cut.FindAll("[data-testid='statement-row']").Should().HaveCount(2);
    }

    [Fact]
    public void FinancialStatementPage_WhenEmpty_ShouldRenderEmptyState()
    {
        // Arrange
        var mockService = new Mock<IFinancialApiService>();
        var dto = new FinancialStatementDto
        {
            PeriodYear = 2026,
            PeriodMonth = 5,
            TotalIncomes = 0m,
            TotalExpenses = 0m,
            NetBalance = 0m,
            PublishedAtUtc = DateTime.UtcNow,
            Items = new List<FinancialStatementItemDto>()
        };

        mockService
            .Setup(x => x.GetFinancialStatementAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        Services.AddSingleton(mockService.Object);
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = RenderComponent<FinancialStatement>();

        // Assert
        cut.Find("[data-testid='empty-statement']").TextContent.Should().Contain("Nenhum lançamento no período");
    }
}
