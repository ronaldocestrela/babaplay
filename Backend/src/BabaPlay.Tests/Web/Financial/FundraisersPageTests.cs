using System;
using System.Collections.Generic;
using System.Threading;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using BabaPlay.Web.Pages.Financial;
using BabaPlay.Web.Models;
using BabaPlay.Web.Services.Http;

namespace BabaPlay.Tests.Web.Financial;

public class FundraisersPageTests : TestContext
{
    [Fact]
    public void FundraisersPage_ShouldRenderCardsGrid()
    {
        // Arrange
        var mockService = new Mock<IFinancialApiService>();
        var list = new List<FundraiserDto>
        {
            new FundraiserDto
            {
                Id = Guid.NewGuid(),
                Title = "Churrasco Fim de Ano",
                TargetAmount = 1000m,
                CollectedAmount = 500m,
                ProgressPercentage = 50.0,
                Status = 0
            },
            new FundraiserDto
            {
                Id = Guid.NewGuid(),
                Title = "Bolas Novas",
                TargetAmount = 400m,
                CollectedAmount = 100m,
                ProgressPercentage = 25.0,
                Status = 0
            }
        };

        mockService
            .Setup(x => x.GetFundraisersAsync(It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<Fundraisers>();

        // Assert
        cut.FindAll("[data-testid='fundraiser-card']").Should().HaveCount(2);
    }

    [Fact]
    public void FundraisersPage_WhenEmpty_ShouldRenderEmptyState()
    {
        // Arrange
        var mockService = new Mock<IFinancialApiService>();

        mockService
            .Setup(x => x.GetFundraisersAsync(It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FundraiserDto>());

        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<Fundraisers>();

        // Assert
        cut.Find("[data-testid='empty-fundraisers']").TextContent.Should().Contain("Nenhuma arrecadação no momento");
    }
}
