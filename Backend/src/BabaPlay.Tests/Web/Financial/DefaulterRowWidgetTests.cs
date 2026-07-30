using System;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using Xunit;
using BabaPlay.Web.Components.Financial;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Financial;

public class DefaulterRowWidgetTests : TestContext
{
    [Fact]
    public void DefaulterRowWidget_ShouldRenderMemberDetailsAndRiskBadgeLeve()
    {
        // Arrange
        var member = new DefaulterMemberDto
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Carlos Silva",
            OverdueInvoicesCount = 2,
            TotalOverdueAmount = 200m,
            MaxDaysOverdue = 15,
            OverdueCompetences = new() { "04/2026", "05/2026" }
        };

        // Act
        var cut = RenderComponent<DefaulterRowWidget>(parameters => parameters
            .Add(p => p.Member, member));

        // Assert
        cut.Find("[data-testid='defaulter-name']").TextContent.Should().Contain("Carlos Silva");
        cut.Find("[data-testid='overdue-count-badge']").TextContent.Should().Contain("2 faturas");
        cut.Find("[data-testid='defaulter-amount']").TextContent.Should().Contain("200,00");
        cut.Find("[data-testid='risk-badge-leve']").TextContent.Should().Contain("Leve (15 d)");
    }

    [Fact]
    public void DefaulterRowWidget_ShouldRenderRiskBadgeCritico()
    {
        // Arrange
        var member = new DefaulterMemberDto
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = "Bruno Santos",
            OverdueInvoicesCount = 3,
            TotalOverdueAmount = 450m,
            MaxDaysOverdue = 75,
            OverdueCompetences = new() { "03/2026", "04/2026", "05/2026" }
        };

        // Act
        var cut = RenderComponent<DefaulterRowWidget>(parameters => parameters
            .Add(p => p.Member, member));

        // Assert
        cut.Find("[data-testid='risk-badge-critico']").TextContent.Should().Contain("Crítico (75 d)");
    }

    [Fact]
    public async Task DefaulterRowWidget_ClickSendReminder_ShouldTriggerCallback()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var member = new DefaulterMemberDto
        {
            PlayerId = playerId,
            PlayerName = "Carlos Silva",
            OverdueInvoicesCount = 1,
            TotalOverdueAmount = 100m,
            MaxDaysOverdue = 10,
            OverdueCompetences = new() { "05/2026" }
        };

        Guid? callbackPlayerId = null;

        // Act
        var cut = RenderComponent<DefaulterRowWidget>(parameters => parameters
            .Add(p => p.Member, member)
            .Add(p => p.OnSendReminder, (Guid id) => callbackPlayerId = id));

        var btn = cut.Find("[data-testid='btn-send-reminder']");
        await btn.ClickAsync(new MouseEventArgs());

        // Assert
        callbackPlayerId.Should().Be(playerId);
    }
}
