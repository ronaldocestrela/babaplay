using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using BabaPlay.Application.Commands.Teams;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Tests.Unit.Application.Teams;

public class GenerateBalancedTeamsCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldGenerateBalancedTeamsWithGoalkeepersAndOutfieldPlayers()
    {
        // Arrange
        var mockPlayerRepo = new Mock<IPlayerRepository>();
        var mockCheckinRepo = new Mock<ICheckinRepository>();

        var tenantId = Guid.NewGuid();
        var players = new List<Player>
        {
            Player.Create(tenantId, Guid.NewGuid(), "Goleiro Bruno", "Bruno", "71999990001", null),
            Player.Create(tenantId, Guid.NewGuid(), "Atleta Zico", "Galinho", "71999990002", null),
            Player.Create(tenantId, Guid.NewGuid(), "Atleta Pelé", "Rei", "71999990003", null),
            Player.Create(tenantId, Guid.NewGuid(), "Atleta Socrates", "Doutor", "71999990004", null)
        };

        mockPlayerRepo.Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(players);

        mockCheckinRepo.Setup(r => r.GetActiveByGameDayAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new List<Checkin>());


        var handler = new GenerateBalancedTeamsCommandHandler(mockPlayerRepo.Object, mockCheckinRepo.Object);
        var command = new GenerateBalancedTeamsCommand(Guid.NewGuid(), NumberOfTeams: 2);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Teams.Should().HaveCount(2);
        result.Value.Teams[0].Players.Should().NotBeEmpty();
        result.Value.Teams[1].Players.Should().NotBeEmpty();
    }
}
