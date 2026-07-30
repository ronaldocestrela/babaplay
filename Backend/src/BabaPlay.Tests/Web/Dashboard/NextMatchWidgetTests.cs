using System;
using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Dashboard;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Dashboard;

public class NextMatchWidgetTests : TestContext
{
    [Fact]
    public void NextMatchWidget_WhenNoMatch_ShouldRenderEmptyState()
    {
        // Act
        var cut = RenderComponent<NextMatchWidget>(parameters => parameters
            .Add(p => p.NextMatch, null));

        // Assert
        cut.Find("[data-testid='no-upcoming-match']").Should().NotBeNull();
        cut.Markup.Should().Contain("Nenhum baba agendado no momento");
    }

    [Fact]
    public void NextMatchWidget_WithMatch_ShouldRenderMatchDetailsAndConfirmedProgress()
    {
        // Arrange
        var nextMatch = new NextMatchWidgetDto(
            Guid.NewGuid(),
            "Baba de Terça",
            DateTime.Now.AddDays(2),
            "Arena Central",
            12,
            20,
            "Confirmed",
            true
        );

        // Act
        var cut = RenderComponent<NextMatchWidget>(parameters => parameters
            .Add(p => p.NextMatch, nextMatch));

        // Assert
        cut.Markup.Should().Contain("Baba de Terça");
        cut.Markup.Should().Contain("12 / 20 vagas");
        cut.Find("[data-testid='user-rsvp-badge']").TextContent.Should().Contain("Vou jogar!");
    }

    [Fact]
    public void NextMatchWidget_WhenRsvpButtonClicked_ShouldTriggerCallback()
    {
        // Arrange
        var nextMatch = new NextMatchWidgetDto(
            Guid.NewGuid(),
            "Baba das Quintas",
            DateTime.Now.AddDays(1),
            "Campo Synthetic",
            8,
            16,
            null,
            true
        );

        bool callbackTriggered = false;
        bool? rsvpStatus = null;

        // Act
        var cut = RenderComponent<NextMatchWidget>(parameters => parameters
            .Add(p => p.NextMatch, nextMatch)
            .Add(p => p.OnRsvpStatusChanged, (bool status) =>
            {
                callbackTriggered = true;
                rsvpStatus = status;
            }));

        var confirmBtn = cut.Find("button.btn-outline-success");
        confirmBtn.Click();

        // Assert
        callbackTriggered.Should().BeTrue();
        rsvpStatus.Should().BeTrue();
    }
}
