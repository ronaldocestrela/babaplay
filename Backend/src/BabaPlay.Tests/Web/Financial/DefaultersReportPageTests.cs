using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using BabaPlay.Web.Pages.Financial;
using BabaPlay.Web.Models;
using BabaPlay.Web.Services.Http;

namespace BabaPlay.Tests.Web.Financial;

public class DefaultersReportPageTests : TestContext
{
    [Fact]
    public void DefaultersReportPage_ShouldRenderKpisAndDefaultersTable()
    {
        // Arrange
        var mockService = new Mock<IFinancialApiService>();
        var dto = new DefaultersListDto
        {
            ReferenceUtc = DateTime.UtcNow,
            TotalDefaultersCount = 2,
            TotalOverdueAmount = 350m,
            AverageDaysOverdue = 40.5,
            Items = new List<DefaulterMemberDto>
            {
                new DefaulterMemberDto
                {
                    PlayerId = Guid.NewGuid(),
                    PlayerName = "Carlos Silva",
                    OverdueInvoicesCount = 2,
                    TotalOverdueAmount = 200m,
                    MaxDaysOverdue = 50,
                    OverdueCompetences = new() { "04/2026", "05/2026" }
                },
                new DefaulterMemberDto
                {
                    PlayerId = Guid.NewGuid(),
                    PlayerName = "Bruno Santos",
                    OverdueInvoicesCount = 1,
                    TotalOverdueAmount = 150m,
                    MaxDaysOverdue = 31,
                    OverdueCompetences = new() { "05/2026" }
                }
            }
        };

        mockService
            .Setup(x => x.GetDefaultersAsync(It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<DefaultersReport>();

        // Assert
        cut.Find("[data-testid='kpi-defaulters-count']").TextContent.Should().Contain("2");
        cut.Find("[data-testid='kpi-total-overdue-amount']").TextContent.Should().Contain("350,00");
        cut.FindAll("[data-testid='defaulter-row']").Should().HaveCount(2);
    }

    [Fact]
    public void DefaultersReportPage_WhenEmpty_ShouldRenderEmptyState()
    {
        // Arrange
        var mockService = new Mock<IFinancialApiService>();
        var dto = new DefaultersListDto
        {
            ReferenceUtc = DateTime.UtcNow,
            TotalDefaultersCount = 0,
            TotalOverdueAmount = 0m,
            AverageDaysOverdue = 0,
            Items = new List<DefaulterMemberDto>()
        };

        mockService
            .Setup(x => x.GetDefaultersAsync(It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<DefaultersReport>();

        // Assert
        cut.Find("[data-testid='empty-defaulters']").TextContent.Should().Contain("Nenhum Atleta Inadimplente!");
    }
}
