using System;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using Xunit;
using BabaPlay.Web.Components.Financial;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Financial;

public class FundraiserCardTests : TestContext
{
    [Fact]
    public void FundraiserCard_ShouldRenderProgressBarAndMetrics()
    {
        // Arrange
        var item = new FundraiserDto
        {
            Id = Guid.NewGuid(),
            Title = "Churrasco Fim de Ano",
            Description = "Vaquinha para compra de carnes e bebidas",
            TargetAmount = 1000m,
            CollectedAmount = 500m,
            ProgressPercentage = 50.0,
            Status = 0,
            ContributionsCount = 10
        };

        // Act
        var cut = RenderComponent<FundraiserCard>(parameters => parameters
            .Add(p => p.Fundraiser, item));

        // Assert
        cut.Find("[data-testid='fundraiser-title']").TextContent.Should().Contain("Churrasco Fim de Ano");
        cut.Find("[data-testid='progress-percentage']").TextContent.Should().Contain("50%");
        cut.Find("[data-testid='collected-amount']").TextContent.Should().Contain("500,00");
        cut.Find("[data-testid='target-amount']").TextContent.Should().Contain("1.000,00");
        cut.Find("[data-testid='status-badge-active']").TextContent.Should().Contain("Em Andamento");
    }

    [Fact]
    public async Task FundraiserCard_ClickContribute_ShouldTriggerCallback()
    {
        // Arrange
        var item = new FundraiserDto
        {
            Id = Guid.NewGuid(),
            Title = "Bolas Novas",
            TargetAmount = 500m,
            CollectedAmount = 200m,
            ProgressPercentage = 40.0,
            Status = 0
        };

        FundraiserDto? callbackItem = null;

        // Act
        var cut = RenderComponent<FundraiserCard>(parameters => parameters
            .Add(p => p.Fundraiser, item)
            .Add(p => p.OnContribute, (FundraiserDto dto) => callbackItem = dto));

        var btn = cut.Find("[data-testid='btn-contribute']");
        await btn.ClickAsync(new MouseEventArgs());

        // Assert
        callbackItem.Should().NotBeNull();
        callbackItem!.Title.Should().Be("Bolas Novas");
    }
}
