using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.MatchEvents;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.MatchEvents;

public class GetMatchEventsByMatchQueryHandlerTests
{
    private readonly Mock<IMatchEventRepository> _eventRepository = new();
    private readonly GetMatchEventsByMatchQueryHandler _handler;

    public GetMatchEventsByMatchQueryHandlerTests()
    {
        _handler = new GetMatchEventsByMatchQueryHandler(_eventRepository.Object);
    }

    [Fact]
    public async Task Handle_EmptyList_ShouldReturnEmpty()
    {
        _eventRepository
            .Setup(x => x.GetActiveByMatchAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.HandleAsync(new GetMatchEventsByMatchQuery(Guid.NewGuid()));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithData_ShouldMapAll()
    {
        var matchId = Guid.NewGuid();
        var first = MatchEvent.Create(Guid.NewGuid(), matchId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10, null);
        var second = MatchEvent.Create(Guid.NewGuid(), matchId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 30, "card");

        _eventRepository
            .Setup(x => x.GetActiveByMatchAsync(matchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([first, second]);

        var result = await _handler.HandleAsync(new GetMatchEventsByMatchQuery(matchId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }
}
