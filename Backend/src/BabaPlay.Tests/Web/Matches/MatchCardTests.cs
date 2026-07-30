using System;
using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Matches;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Matches;

public class MatchCardTests : TestContext
{
    [Fact]
    public void MatchCard_ShouldRenderMatchInformationAndStatusBadge()
    {
        // Arrange
        var match = new MatchDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Baba Amigos do Futebol",
            DateTime.Now.AddDays(2),
            "Arena Central",
            "Jogo das 20h",
            "Time Vermelho",
            "Time Azul",
            "Confirmed",
            20,
            14
        );

        // Act
        var cut = RenderComponent<MatchCard>(parameters => parameters
            .Add(p => p.Match, match));

        // Assert
        cut.Find("[data-testid='match-title']").TextContent.Should().Contain("Baba Amigos do Futebol");
        cut.Markup.Should().Contain("Arena Central");
        cut.Find(".vagas-count").TextContent.Should().Contain("14");
        cut.Find(".vagas-count").TextContent.Should().Contain("20 vagas");
        cut.Find("[data-testid='match-status-badge']").TextContent.Should().Contain("Agendado");
    }

    [Fact]
    public void MatchCard_WhenEditButtonClicked_ShouldTriggerOnEditCallback()
    {
        // Arrange
        var match = new MatchDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Baba de Quinta",
            DateTime.Now.AddDays(1),
            "Campo 2",
            null,
            null,
            null,
            "Confirmed",
            16,
            10
        );

        MatchDto? editedMatch = null;

        // Act
        var cut = RenderComponent<MatchCard>(parameters => parameters
            .Add(p => p.Match, match)
            .Add(p => p.OnEdit, (MatchDto m) => editedMatch = m));

        var editButton = cut.Find("[data-testid='btn-edit-match']");
        editButton.Click();

        // Assert
        editedMatch.Should().NotBeNull();
        editedMatch!.Id.Should().Be(match.Id);
    }
}
