using System;
using System.Threading;
using Bunit;
using FluentAssertions;
using Moq;
using Xunit;
using BabaPlay.Web.Models;
using BabaPlay.Web.Pages.Players;
using BabaPlay.Web.Services.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Tests.Web.Players;

public class DigitalIdCardPageTests : TestContext
{
    [Fact]
    public void DigitalIdCardPage_ShouldLoadAndRenderCard()
    {
        // Arrange
        var mockService = new Mock<IPlayerApiService>();
        var card = new DigitalCardDto(
            PlayerId: Guid.NewGuid(),
            CardNumber: "BP-55667788",
            AssociationName: "Baba Play FC",
            AssociationLogoUrl: null,
            PlayerName: "Rodrygo Goes",
            Nickname: "Rayon",
            PhotoUrl: null,
            PrimaryPositionName: "Atacante",
            JerseyNumber: 11,
            RoleName: "Atleta",
            MemberSince: DateTime.UtcNow,
            ValidUntil: DateTime.UtcNow.AddYears(1),
            QrCodeData: "BABAPLAY:CARD:RODRYGO",
            IsActive: true);

        mockService
            .Setup(x => x.GetDigitalIdCardAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        Services.AddSingleton(mockService.Object);
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = RenderComponent<DigitalIdCard>();

        // Assert
        cut.Find("[data-testid='digital-id-card-page']").Should().NotBeNull();
        cut.Find("[data-testid='digital-id-card-widget']").Should().NotBeNull();
        cut.Markup.Should().Contain("Rodrygo Goes");
    }

    [Fact]
    public void DigitalIdCardPage_WhenCardNotFound_ShouldShowErrorState()
    {
        // Arrange
        var mockService = new Mock<IPlayerApiService>();

        mockService
            .Setup(x => x.GetDigitalIdCardAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DigitalCardDto?)null);

        Services.AddSingleton(mockService.Object);
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = RenderComponent<DigitalIdCard>();

        // Assert
        cut.Find("[data-testid='error-card-state']").Should().NotBeNull();
        cut.Markup.Should().Contain("Carteirinha Não Encontrada");
    }
}
