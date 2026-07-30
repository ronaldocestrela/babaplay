using System;
using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Matches;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Matches;

public class RsvpStatusWidgetTests : TestContext
{
    [Fact]
    public void RsvpStatusWidget_ShouldRenderGameDayInformationAndBadge()
    {
        // Arrange
        var summary = new GameDayRsvpSummaryDto(
            Guid.NewGuid(),
            "Baba de Sábado Espetacular",
            DateTime.Now.AddDays(2),
            "Arena da Ilha",
            20,
            15,
            0,
            "Confirmed",
            null
        );

        // Act
        var cut = RenderComponent<RsvpStatusWidget>(parameters => parameters
            .Add(p => p.Summary, summary));

        // Assert
        cut.Find("[data-testid='gameday-title']").TextContent.Should().Contain("Baba de Sábado Espetacular");
        cut.Find("[data-testid='user-status-badge']").TextContent.Should().Contain("Presença Confirmada");
        cut.Markup.Should().Contain("15 / 20 confirmados");
    }

    [Fact]
    public void RsvpStatusWidget_WhenWaitlistActive_ShouldDisplayWaitlistAlertAndBadge()
    {
        // Arrange
        var summary = new GameDayRsvpSummaryDto(
            Guid.NewGuid(),
            "Baba Lotado",
            DateTime.Now.AddDays(1),
            "Campo Principal",
            20,
            20,
            3,
            "Waitlist",
            2
        );

        // Act
        var cut = RenderComponent<RsvpStatusWidget>(parameters => parameters
            .Add(p => p.Summary, summary));

        // Assert
        cut.Find("[data-testid='user-status-badge']").TextContent.Should().Contain("Lista de Espera");
        cut.Find("[data-testid='waitlist-indicator']").TextContent.Should().Contain("3 atleta(s) aguardando vaga");
        cut.Find("[data-testid='waitlist-position-alert']").TextContent.Should().Contain("Posição #2");
    }

    [Fact]
    public void RsvpStatusWidget_WhenConfirmButtonClicked_ShouldTriggerCallback()
    {
        // Arrange
        var summary = new GameDayRsvpSummaryDto(
            Guid.NewGuid(),
            "Baba Amigos",
            DateTime.Now.AddDays(1),
            "Campo 1",
            20,
            10,
            0,
            null,
            null
        );

        bool? isAttending = null;

        // Act
        var cut = RenderComponent<RsvpStatusWidget>(parameters => parameters
            .Add(p => p.Summary, summary)
            .Add(p => p.OnRsvpSubmitted, (bool status) => isAttending = status));

        var confirmButton = cut.Find("[data-testid='btn-confirm-rsvp']");
        confirmButton.Click();

        // Assert
        isAttending.Should().BeTrue();
    }
}
