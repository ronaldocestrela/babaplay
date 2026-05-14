using BabaPlay.Application.Commands.Matches;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using FluentAssertions;
using Moq;
using DomainMatch = BabaPlay.Domain.Entities.Match;

namespace BabaPlay.Tests.Unit.Application.Matches;

public class ChangeMatchStatusCommandHandlerTests
{
    private readonly Mock<IMatchRepository> _matchRepository = new();
    private readonly ChangeMatchStatusCommandHandler _handler;

    public ChangeMatchStatusCommandHandlerTests()
    {
        _handler = new ChangeMatchStatusCommandHandler(_matchRepository.Object);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldReturnMatchNotFound()
    {
        _matchRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainMatch?)null);

        var result = await _handler.HandleAsync(new ChangeMatchStatusCommand(Guid.NewGuid(), MatchStatus.Scheduled));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ValidTransition_ShouldUpdateStatus()
    {
        var match = DomainMatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);
        _matchRepository.Setup(x => x.GetByIdAsync(match.Id, It.IsAny<CancellationToken>())).ReturnsAsync(match);

        var result = await _handler.HandleAsync(new ChangeMatchStatusCommand(match.Id, MatchStatus.Scheduled));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(MatchStatus.Scheduled);
    }
}