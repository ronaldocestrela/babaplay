using System;
using System.Collections.Generic;
using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Players;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Players;

public class EditPlayerModalTests : TestContext
{
    [Fact]
    public void EditPlayerModal_WhenIsOpenFalse_ShouldNotRenderModal()
    {
        // Act
        var cut = RenderComponent<EditPlayerModal>(parameters => parameters
            .Add(p => p.IsOpen, false));

        // Assert
        cut.FindAll("[data-testid='edit-player-modal']").Should().BeEmpty();
    }

    [Fact]
    public void EditPlayerModal_WhenIsOpenTrue_ShouldPopulatePlayerValues()
    {
        // Arrange
        var player = new PlayerDto(
            Id: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Name: "Lucas Paquetá",
            Nickname: "Paquetá",
            Phone: "(21) 98888-7777",
            DateOfBirth: new DateTime(1997, 8, 27),
            PreferredFoot: "Left",
            PrimaryPositionId: Guid.NewGuid(),
            PrimaryPositionName: "Meia",
            JerseyNumber: 11,
            PhotoUrl: null,
            RoleId: null,
            RoleName: null,
            IsActive: true,
            CreatedAt: DateTime.UtcNow);

        // Act
        var cut = RenderComponent<EditPlayerModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Player, player));

        // Assert
        cut.Find("[data-testid='edit-player-modal']").Should().NotBeNull();
        cut.Markup.Should().Contain("Lucas Paquetá");
    }

    [Fact]
    public void EditPlayerModal_WhenFormSubmitted_ShouldTriggerOnSave()
    {
        // Arrange
        var player = new PlayerDto(
            Id: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Name: "Casemiro",
            Nickname: "Case",
            Phone: null,
            DateOfBirth: null,
            PreferredFoot: "Right",
            PrimaryPositionId: null,
            PrimaryPositionName: "Volante",
            JerseyNumber: 5,
            PhotoUrl: null,
            RoleId: null,
            RoleName: null,
            IsActive: true,
            CreatedAt: DateTime.UtcNow);

        UpdatePlayerAdminDto? savedDto = null;

        var cut = RenderComponent<EditPlayerModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Player, player)
            .Add(p => p.OnSave, (UpdatePlayerAdminDto dto) => savedDto = dto));

        // Act
        var form = cut.Find("form");
        form.Submit();

        // Assert
        savedDto.Should().NotBeNull();
        savedDto!.Name.Should().Be("Casemiro");
    }
}
