using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Matches;
using DomainMatch = BabaPlay.Domain.Entities.Match;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Tests.Unit.Application.Matches;

public class GetMatchStatsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenMatchNotFound_ShouldReturnFail()
    {
        // Arrange
        var mockMatchRepo = new Mock<IMatchRepository>();
        var mockGameDayRepo = new Mock<IGameDayRepository>();
        var mockTeamRepo = new Mock<ITeamRepository>();
        var mockPlayerRepo = new Mock<IPlayerRepository>();
        var matchId = Guid.NewGuid();

        mockMatchRepo.Setup(r => r.GetByIdAsync(matchId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((DomainMatch?)null);
        mockGameDayRepo.Setup(r => r.GetByIdAsync(matchId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync((GameDay?)null);

        var handler = new GetMatchStatsQueryHandler(
            mockMatchRepo.Object,
            mockGameDayRepo.Object,
            mockTeamRepo.Object,
            mockPlayerRepo.Object);

        var query = new GetMatchStatsQuery(matchId);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_NOT_FOUND");
    }

    [Fact]
    public async Task HandleAsync_WhenMatchExistsWithTeamsAndPlayers_ShouldReturnRealPlayerStats()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var gameDayId = Guid.NewGuid();
        var homeTeamId = Guid.NewGuid();
        var awayTeamId = Guid.NewGuid();
        var matchId = Guid.NewGuid();

        var match = DomainMatch.Create(tenantId, gameDayId, homeTeamId, awayTeamId, "Partida Retroativa");

        var homePlayer = Player.Create(tenantId, Guid.NewGuid(), "Carlos Silva", "Carlinhos", "71999990001", null);
        var awayPlayer = Player.Create(tenantId, Guid.NewGuid(), "Eduardo Ramos", "Dudu", "71999990002", null);

        var homeTeam = Team.Create(tenantId, "Time Branco", 10);
        homeTeam.SetPlayers(new[] { homePlayer.Id }, hasGoalkeeper: true);

        var awayTeam = Team.Create(tenantId, "Time Preto", 10);
        awayTeam.SetPlayers(new[] { awayPlayer.Id }, hasGoalkeeper: true);

        var mockMatchRepo = new Mock<IMatchRepository>();
        var mockGameDayRepo = new Mock<IGameDayRepository>();
        var mockTeamRepo = new Mock<ITeamRepository>();
        var mockPlayerRepo = new Mock<IPlayerRepository>();

        mockMatchRepo.Setup(r => r.GetByIdAsync(matchId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(match);
        mockTeamRepo.Setup(r => r.GetByIdAsync(homeTeamId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(homeTeam);
        mockTeamRepo.Setup(r => r.GetByIdAsync(awayTeamId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(awayTeam);
        mockPlayerRepo.Setup(r => r.GetByIdAsync(homePlayer.Id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(homePlayer);
        mockPlayerRepo.Setup(r => r.GetByIdAsync(awayPlayer.Id, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(awayPlayer);

        var handler = new GetMatchStatsQueryHandler(
            mockMatchRepo.Object,
            mockGameDayRepo.Object,
            mockTeamRepo.Object,
            mockPlayerRepo.Object);

        var query = new GetMatchStatsQuery(matchId);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.MatchId.Should().Be(match.Id);
        result.Value.PlayerStats.Should().HaveCount(2);
        result.Value.PlayerStats.Should().Contain(p => p.PlayerId == homePlayer.Id && p.PlayerName == "Carlos Silva" && p.TeamId == homeTeamId);
        result.Value.PlayerStats.Should().Contain(p => p.PlayerId == awayPlayer.Id && p.PlayerName == "Eduardo Ramos" && p.TeamId == awayTeamId);
    }
}
