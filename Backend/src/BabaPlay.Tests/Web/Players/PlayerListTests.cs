using System;
using System.Collections.Generic;
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

public class PlayerListTests : TestContext
{
    [Fact]
    public void PlayerList_ShouldRenderPlayersAndAllowFiltering()
    {
        // Arrange
        var mockPlayerService = new Mock<IPlayerApiService>();
        
        var posAttacker = new PositionDto(Guid.NewGuid(), "Atacante", "ATA");
        var posDefender = new PositionDto(Guid.NewGuid(), "Zagueiro", "ZAG");

        var players = new List<PlayerDto>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Vinicius Jr", "Vini", "(11) 99999-1111", null, "Right", posAttacker.Id, "Atacante", 7, null, null, "Atleta", true, DateTime.UtcNow),
            new(Guid.NewGuid(), Guid.NewGuid(), "Thiago Silva", "Monstro", "(21) 98888-2222", null, "Right", posDefender.Id, "Zagueiro", 3, null, null, "Treinador", true, DateTime.UtcNow),
            new(Guid.NewGuid(), Guid.NewGuid(), "Jogador Inativo", "Ex", null, null, "Left", null, null, null, null, null, null, false, DateTime.UtcNow)
        };

        mockPlayerService
            .Setup(x => x.GetPlayersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(players);

        mockPlayerService
            .Setup(x => x.GetPositionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PositionDto> { posAttacker, posDefender });

        mockPlayerService
            .Setup(x => x.GetRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleDto>());

        Services.AddSingleton(mockPlayerService.Object);

        // Act
        var cut = RenderComponent<PlayerList>();

        // Assert
        cut.Markup.Should().Contain("Vinicius Jr");
        cut.Markup.Should().Contain("Thiago Silva");
        cut.Markup.Should().Contain("Jogador Inativo");

        // Filter by Search Term "Vini"
        var searchInput = cut.Find("[data-testid='search-input']");
        searchInput.Input("Vini");

        cut.Markup.Should().Contain("Vinicius Jr");
        cut.Markup.Should().NotContain("Thiago Silva");
    }

    [Fact]
    public void PlayerList_StatusFilterActive_ShouldFilterOutInactivePlayers()
    {
        // Arrange
        var mockPlayerService = new Mock<IPlayerApiService>();

        var players = new List<PlayerDto>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Atleta Ativo", "Ativo", null, null, "Right", null, "Atacante", 10, null, null, "Atleta", true, DateTime.UtcNow),
            new(Guid.NewGuid(), Guid.NewGuid(), "Atleta Inativo", "Inativo", null, null, "Left", null, "Zagueiro", 5, null, null, "Atleta", false, DateTime.UtcNow)
        };

        mockPlayerService
            .Setup(x => x.GetPlayersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(players);

        mockPlayerService
            .Setup(x => x.GetPositionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PositionDto>());

        mockPlayerService
            .Setup(x => x.GetRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleDto>());

        Services.AddSingleton(mockPlayerService.Object);

        // Act
        var cut = RenderComponent<PlayerList>();

        var statusFilter = cut.Find("[data-testid='status-filter']");
        statusFilter.Change("Active");

        // Assert
        cut.Markup.Should().Contain("Atleta Ativo");
        cut.Markup.Should().NotContain("Atleta Inativo");
    }
}
