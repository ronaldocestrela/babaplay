using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using BabaPlay.Application.Commands.Matches;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using DomainMatch = BabaPlay.Domain.Entities.Match;

namespace BabaPlay.Tests.Unit.Application.Matches;

public class RegisterMatchStatsCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenMatchExists_ShouldRegisterStatsSuccessfully()
    {
        // Arrange
        var mockMatchRepo = new Mock<IMatchRepository>();
        var matchId = Guid.NewGuid();
        var match = DomainMatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Baba Semanal");


        mockMatchRepo.Setup(r => r.GetByIdAsync(matchId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(match);

        var handler = new RegisterMatchStatsCommandHandler(mockMatchRepo.Object);
        var stats = new List<PlayerMatchStatsApplicationDto>
        {
            new(Guid.NewGuid(), "Zico", null, Guid.NewGuid(), 2, 1, 0, 0, 0)
        };

        var command = new RegisterMatchStatsCommand(matchId, 2, 1, stats);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.HomeScore.Should().Be(2);
        result.Value.AwayScore.Should().Be(1);
        result.Value.PlayerStats.Should().HaveCount(1);
    }
}
