using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using BabaPlay.Application.Commands.Matches;
using BabaPlay.Application.Interfaces;
using DomainMatch = BabaPlay.Domain.Entities.Match;

namespace BabaPlay.Tests.Unit.Application.Matches;

public class SubmitMvpVoteCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenVoterIsSameAsCandidate_ShouldReturnSelfVoteNotAllowedError()
    {
        // Arrange
        var mockMatchRepo = new Mock<IMatchRepository>();
        var playerId = Guid.NewGuid();

        var handler = new SubmitMvpVoteCommandHandler(mockMatchRepo.Object);
        var command = new SubmitMvpVoteCommand(Guid.NewGuid(), VoterPlayerId: playerId, CandidatePlayerId: playerId);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("SELF_VOTE_NOT_ALLOWED");
        result.ErrorMessage.Should().Contain("Você não pode votar em si mesmo");
    }

    [Fact]
    public async Task HandleAsync_WhenValidVote_ShouldReturnSuccess()
    {
        // Arrange
        var mockMatchRepo = new Mock<IMatchRepository>();
        var matchId = Guid.NewGuid();
        var match = DomainMatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Baba");

        mockMatchRepo.Setup(r => r.GetByIdAsync(matchId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(match);

        var handler = new SubmitMvpVoteCommandHandler(mockMatchRepo.Object);
        var command = new SubmitMvpVoteCommand(matchId, VoterPlayerId: Guid.NewGuid(), CandidatePlayerId: Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
