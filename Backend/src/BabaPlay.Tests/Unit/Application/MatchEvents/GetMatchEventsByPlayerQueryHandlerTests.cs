using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.MatchEvents;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.MatchEvents;

public class GetMatchEventsByPlayerQueryHandlerTests
{
    private readonly Mock<IMatchEventRepository> _eventRepository = new();
    private readonly GetMatchEventsByPlayerQueryHandler _handler;

    public GetMatchEventsByPlayerQueryHandlerTests()
    {
        _handler = new GetMatchEventsByPlayerQueryHandler(_eventRepository.Object);
    }

    [Fact]
    public async Task Handle_EmptyList_ShouldReturnEmpty()
    {
        _eventRepository
            .Setup(x => x.GetActiveByPlayerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.HandleAsync(new GetMatchEventsByPlayerQuery(Guid.NewGuid()));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithData_ShouldReturnMappedItems()
    {
        var playerId = Guid.NewGuid();
        var first = MatchEvent.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), playerId, Guid.NewGuid(), 5, null);
        var second = MatchEvent.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), playerId, Guid.NewGuid(), 80, "late goal");

        _eventRepository
            .Setup(x => x.GetActiveByPlayerAsync(playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([first, second]);

        var result = await _handler.HandleAsync(new GetMatchEventsByPlayerQuery(playerId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }
}
