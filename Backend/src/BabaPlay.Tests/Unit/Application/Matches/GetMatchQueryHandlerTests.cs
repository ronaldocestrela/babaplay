using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Matches;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;
using DomainMatch = BabaPlay.Domain.Entities.Match;

namespace BabaPlay.Tests.Unit.Application.Matches;

public class GetMatchQueryHandlerTests
{
    private readonly Mock<IMatchRepository> _matchRepository = new();
    private readonly GetMatchQueryHandler _handler;

    public GetMatchQueryHandlerTests()
    {
        _handler = new GetMatchQueryHandler(_matchRepository.Object);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldReturnMatchNotFound()
    {
        _matchRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainMatch?)null);

        var result = await _handler.HandleAsync(new GetMatchQuery(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ExistingMatch_ShouldReturnResponse()
    {
        var match = DomainMatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Clássico");
        _matchRepository.Setup(x => x.GetByIdAsync(match.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(match);

        var result = await _handler.HandleAsync(new GetMatchQuery(match.Id));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(match.Id);
    }
}