using System;
using System.Collections.Generic;
using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Financial;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Financial;

public class RecentTransactionsWidgetTests : TestContext
{
    [Fact]
    public void RecentTransactionsWidget_WhenEmpty_ShouldShowEmptyMessage()
    {
        // Act
        var cut = RenderComponent<RecentTransactionsWidget>(parameters => parameters
            .Add(p => p.Transactions, new List<RecentTransactionDto>()));

        // Assert
        cut.Markup.Should().Contain("Nenhuma movimentação registrada no período");
    }

    [Fact]
    public void RecentTransactionsWidget_WithTransactions_ShouldRenderList()
    {
        // Arrange
        var transactions = new List<RecentTransactionDto>
        {
            new RecentTransactionDto
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                Type = 1, // Income
                Amount = 250m,
                Description = "Mensalidade Atleta João",
                OccurredOnUtc = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc)
            },
            new RecentTransactionDto
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                Type = 2, // Expense
                Amount = 80m,
                Description = "Compra de Bolas",
                OccurredOnUtc = new DateTime(2026, 5, 12, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        // Act
        var cut = RenderComponent<RecentTransactionsWidget>(parameters => parameters
            .Add(p => p.Transactions, transactions));

        // Assert
        cut.Markup.Should().Contain("Mensalidade Atleta João");
        cut.Markup.Should().Contain("Entrada");
        cut.Markup.Should().Contain("250,00");

        cut.Markup.Should().Contain("Compra de Bolas");
        cut.Markup.Should().Contain("Saída");
        cut.Markup.Should().Contain("80,00");
    }
}
