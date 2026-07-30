using System;
using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Players;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Players;

public class PlayerCardTests : TestContext
{
    [Fact]
    public void PlayerCard_ShouldRenderPlayerDetailsCorrectly()
    {
        // Arrange
        var player = new PlayerDto(
            Id: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Name: "Neymar Jr",
            Nickname: "Menino Ney",
            Phone: "(11) 99999-8888",
            DateOfBirth: new DateTime(1992, 2, 5),
            PreferredFoot: "Right",
            PrimaryPositionId: Guid.NewGuid(),
            PrimaryPositionName: "Atacante",
            JerseyNumber: 10,
            PhotoUrl: null,
            RoleId: Guid.NewGuid(),
            RoleName: "Atleta",
            IsActive: true,
            CreatedAt: DateTime.UtcNow);

        // Act
        var cut = RenderComponent<PlayerCard>(parameters => parameters
            .Add(p => p.Player, player));

        // Assert
        cut.Markup.Should().Contain("Neymar Jr");
        cut.Markup.Should().Contain("\"Menino Ney\"");
        cut.Markup.Should().Contain("#10");
        cut.Markup.Should().Contain("Atacante");
        cut.Markup.Should().Contain("Direito");
        cut.Markup.Should().Contain("Ativo");
    }

    [Fact]
    public void PlayerCard_WhenEditButtonClicked_ShouldTriggerOnEditCallback()
    {
        // Arrange
        var player = new PlayerDto(
            Id: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Name: "Gabriel Barbosa",
            Nickname: "Gabigol",
            Phone: null,
            DateOfBirth: null,
            PreferredFoot: "Left",
            PrimaryPositionId: null,
            PrimaryPositionName: "Atacante",
            JerseyNumber: 9,
            PhotoUrl: null,
            RoleId: null,
            RoleName: null,
            IsActive: true,
            CreatedAt: DateTime.UtcNow);

        PlayerDto? editedPlayer = null;

        var cut = RenderComponent<PlayerCard>(parameters => parameters
            .Add(p => p.Player, player)
            .Add(p => p.OnEdit, (PlayerDto dto) => editedPlayer = dto));

        // Act
        var editButton = cut.Find("button.btn-edit-player");
        editButton.Click();

        // Assert
        editedPlayer.Should().NotBeNull();
        editedPlayer!.Id.Should().Be(player.Id);
    }
}
