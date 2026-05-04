using BabaPlay.Application.Commands.MatchEvents;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.MatchEvents;

public class DeleteMatchEventCommandHandlerTests
{
    private readonly Mock<IMatchEventRepository> _eventRepository = new();
    private readonly Mock<IMatchEventRealtimeNotifier> _realtimeNotifier = new();
    private readonly DeleteMatchEventCommandHandler _handler;

    public DeleteMatchEventCommandHandlerTests()
    {
        _handler = new DeleteMatchEventCommandHandler(_eventRepository.Object, _realtimeNotifier.Object);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldReturnNotFound()
    {
        _eventRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MatchEvent?)null);

        var result = await _handler.HandleAsync(new DeleteMatchEventCommand(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_EVENT_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_Existing_ShouldDeactivateAndNotify()
    {
        var matchEvent = MatchEvent.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 12, null);

        _eventRepository
            .Setup(x => x.GetByIdAsync(matchEvent.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(matchEvent);

        var result = await _handler.HandleAsync(new DeleteMatchEventCommand(matchEvent.Id));

        result.IsSuccess.Should().BeTrue();
        matchEvent.IsActive.Should().BeFalse();
        _eventRepository.Verify(x => x.UpdateAsync(matchEvent, It.IsAny<CancellationToken>()), Times.Once);
        _eventRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _realtimeNotifier.Verify(x => x.NotifyMatchEventDeletedAsync(matchEvent.MatchId, matchEvent.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
