using System;
using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Players;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Players;

public class DigitalIdCardWidgetTests : TestContext
{
    [Fact]
    public void DigitalIdCardWidget_ShouldRenderMemberAndAssociationDetails()
    {
        // Arrange
        var card = new DigitalCardDto(
            PlayerId: Guid.NewGuid(),
            CardNumber: "BP-99887766",
            AssociationName: "Liga Bahiana de Futebol",
            AssociationLogoUrl: null,
            PlayerName: "Richarlison",
            Nickname: "Pombo",
            PhotoUrl: null,
            PrimaryPositionName: "Atacante",
            JerseyNumber: 9,
            RoleName: "Atleta",
            MemberSince: new DateTime(2025, 1, 1),
            ValidUntil: new DateTime(2026, 12, 31),
            QrCodeData: "BABAPLAY:CARD:TEST",
            IsActive: true);

        // Act
        var cut = RenderComponent<DigitalIdCardWidget>(parameters => parameters
            .Add(p => p.Card, card));

        // Assert
        cut.Find("[data-testid='card-player-name']").TextContent.Should().Be("Richarlison");
        cut.Find("[data-testid='card-number']").TextContent.Should().Be("BP-99887766");
        cut.Find("[data-testid='card-position']").TextContent.Should().Contain("Atacante");
        cut.Find("[data-testid='card-role']").TextContent.Should().Contain("Atleta");
        cut.Find("[data-testid='card-validity']").TextContent.Should().Be("31/12/2026");
        cut.Markup.Should().Contain("Válida");
    }

    [Fact]
    public void DigitalIdCardWidget_WhenFlipButtonClicked_ShouldToggleFlipState()
    {
        // Arrange
        var card = new DigitalCardDto(
            PlayerId: Guid.NewGuid(),
            CardNumber: "BP-11223344",
            AssociationName: "Baba Play FC",
            AssociationLogoUrl: null,
            PlayerName: "Alisson Becker",
            Nickname: "Goleirasso",
            PhotoUrl: null,
            PrimaryPositionName: "Goleiro",
            JerseyNumber: 1,
            RoleName: "Atleta",
            MemberSince: DateTime.UtcNow,
            ValidUntil: DateTime.UtcNow.AddYears(1),
            QrCodeData: "BABAPLAY:CARD:11223344",
            IsActive: true);

        var cut = RenderComponent<DigitalIdCardWidget>(parameters => parameters
            .Add(p => p.Card, card));

        // Act - Front view initially, click flip
        var flipButtonFront = cut.Find("[data-testid='btn-flip-front']");
        flipButtonFront.Click();

        // Assert - Should contain flipped class and show QR code container
        cut.Find(".digital-card-container").ClassList.Should().Contain("flipped");
        cut.Find("[data-testid='card-qr-code']").Should().NotBeNull();

        // Act - Click flip back
        var flipButtonBack = cut.Find("[data-testid='btn-flip-back']");
        flipButtonBack.Click();

        // Assert - Should remove flipped class
        cut.Find(".digital-card-container").ClassList.Should().NotContain("flipped");
    }
}
