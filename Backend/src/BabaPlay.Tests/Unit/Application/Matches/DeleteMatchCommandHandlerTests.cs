using BabaPlay.Application.Commands.Matches;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;
using DomainMatch = BabaPlay.Domain.Entities.Match;

namespace BabaPlay.Tests.Unit.Application.Matches;

public class DeleteMatchCommandHandlerTests
{
    private readonly Mock<IMatchRepository> _matchRepository = new();
    private readonly DeleteMatchCommandHandler _handler;

    public DeleteMatchCommandHandlerTests()
    {
        _handler = new DeleteMatchCommandHandler(_matchRepository.Object);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldReturnMatchNotFound()
    {
        _matchRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainMatch?)null);

        var result = await _handler.HandleAsync(new DeleteMatchCommand(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ExistingMatch_ShouldDeactivateAndReturnOk()
    {
        var match = DomainMatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);
        _matchRepository.Setup(x => x.GetByIdAsync(match.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(match);

        var result = await _handler.HandleAsync(new DeleteMatchCommand(match.Id));

        result.IsSuccess.Should().BeTrue();
        match.IsActive.Should().BeFalse();
        _matchRepository.Verify(x => x.UpdateAsync(match, It.IsAny<CancellationToken>()), Times.Once);
    }
}