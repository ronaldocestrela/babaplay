using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Dashboard;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using DomainMatch = BabaPlay.Domain.Entities.Match;

namespace BabaPlay.Tests.Unit.Application.Dashboard;

public class GetDashboardSummaryQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnConsolidatedDashboardSummary()
    {
        // Arrange
        var mockPlayerRepo = new Mock<IPlayerRepository>();
        var mockMatchRepo = new Mock<IMatchRepository>();
        var mockGameDayRepo = new Mock<IGameDayRepository>();

        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var player = Player.Create(tenantId, userId, "Atleta Teste", "Pelé", "(11) 99999-9999", null);
        mockPlayerRepo
            .Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Player> { player });

        mockMatchRepo
            .Setup(x => x.GetAllActiveAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainMatch>());

        mockGameDayRepo
            .Setup(x => x.GetAllActiveAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameDay>());

        var handler = new GetDashboardSummaryQueryHandler(mockPlayerRepo.Object, mockMatchRepo.Object, mockGameDayRepo.Object);

        // Act
        var result = await handler.HandleAsync(new GetDashboardSummaryQuery());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Stats.ActivePlayersCount.Should().Be(1);
        result.Value.Announcements.Should().NotBeEmpty();
    }
}
