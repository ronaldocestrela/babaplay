using BabaPlay.Application.Commands.MatchEvents;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.MatchEvents;

public class UpdateMatchEventCommandHandlerTests
{
    private readonly Mock<IMatchEventRepository> _eventRepository = new();
    private readonly Mock<IMatchEventTypeRepository> _typeRepository = new();
    private readonly Mock<IMatchEventRealtimeNotifier> _realtimeNotifier = new();
    private readonly UpdateMatchEventCommandHandler _handler;

    public UpdateMatchEventCommandHandlerTests()
    {
        _handler = new UpdateMatchEventCommandHandler(
            _eventRepository.Object,
            _typeRepository.Object,
            _realtimeNotifier.Object);
    }

    [Fact]
    public async Task Handle_EventNotFound_ShouldReturnNotFound()
    {
        _eventRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MatchEvent?)null);

        var result = await _handler.HandleAsync(new UpdateMatchEventCommand(Guid.NewGuid(), Guid.NewGuid(), 10, null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_EVENT_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_TypeInactive_ShouldReturnInactiveError()
    {
        var matchEvent = MatchEvent.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10, null);
        var matchEventType = MatchEventType.Create(Guid.NewGuid(), "goal", "Goal", 2, true);
        matchEventType.Deactivate();

        _eventRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(matchEvent);
        _typeRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(matchEventType);

        var result = await _handler.HandleAsync(new UpdateMatchEventCommand(matchEvent.Id, matchEventType.Id, 25, "updated"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_EVENT_TYPE_INACTIVE");
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldUpdateEventAndNotify()
    {
        var matchEvent = MatchEvent.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10, null);
        var matchEventType = MatchEventType.Create(Guid.NewGuid(), "assist", "Assist", 1, false);

        _eventRepository
            .Setup(x => x.GetByIdAsync(matchEvent.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(matchEvent);
        _typeRepository
            .Setup(x => x.GetByIdAsync(matchEventType.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(matchEventType);

        var result = await _handler.HandleAsync(new UpdateMatchEventCommand(matchEvent.Id, matchEventType.Id, 67, "assist"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Minute.Should().Be(67);
        _eventRepository.Verify(x => x.UpdateAsync(matchEvent, It.IsAny<CancellationToken>()), Times.Once);
        _eventRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _realtimeNotifier.Verify(x => x.NotifyMatchEventUpdatedAsync(matchEvent.MatchId, matchEvent.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
