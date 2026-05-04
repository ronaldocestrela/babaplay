using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.MatchEvents;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.MatchEvents;

public class GetMatchEventQueryHandlerTests
{
    private readonly Mock<IMatchEventRepository> _eventRepository = new();
    private readonly GetMatchEventQueryHandler _handler;

    public GetMatchEventQueryHandlerTests()
    {
        _handler = new GetMatchEventQueryHandler(_eventRepository.Object);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldReturnError()
    {
        _eventRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MatchEvent?)null);

        var result = await _handler.HandleAsync(new GetMatchEventQuery(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_EVENT_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_Existing_ShouldReturnResponse()
    {
        var matchEvent = MatchEvent.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 20, "goal");

        _eventRepository
            .Setup(x => x.GetByIdAsync(matchEvent.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(matchEvent);

        var result = await _handler.HandleAsync(new GetMatchEventQuery(matchEvent.Id));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(matchEvent.Id);
    }
}
