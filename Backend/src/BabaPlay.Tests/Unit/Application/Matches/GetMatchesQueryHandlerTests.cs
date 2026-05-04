using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Matches;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;
using DomainMatch = BabaPlay.Domain.Entities.Match;

namespace BabaPlay.Tests.Unit.Application.Matches;

public class GetMatchesQueryHandlerTests
{
    private readonly Mock<IMatchRepository> _matchRepository = new();
    private readonly GetMatchesQueryHandler _handler;

    public GetMatchesQueryHandlerTests()
    {
        _handler = new GetMatchesQueryHandler(_matchRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnList()
    {
        var matches = new List<DomainMatch>
        {
            DomainMatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null),
        };

        _matchRepository
            .Setup(x => x.GetAllActiveAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(matches);

        var result = await _handler.HandleAsync(new GetMatchesQuery(null));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }
}