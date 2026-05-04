using BabaPlay.Application.Commands.Checkins;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Checkins;

public class CancelCheckinCommandHandlerTests
{
    private readonly Mock<ICheckinRepository> _checkinRepository = new();
    private readonly Mock<ICheckinRealtimeNotifier> _realtimeNotifier = new();
    private readonly CancelCheckinCommandHandler _handler;

    public CancelCheckinCommandHandlerTests()
    {
        _handler = new CancelCheckinCommandHandler(_checkinRepository.Object, _realtimeNotifier.Object);
    }

    [Fact]
    public async Task Handle_CheckinNotFound_ShouldReturnCheckinNotFound()
    {
        _checkinRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Checkin?)null);

        var result = await _handler.HandleAsync(new CancelCheckinCommand(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("CHECKIN_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_AlreadyInactive_ShouldReturnSuccessWithoutRealtimeEvents()
    {
        var checkin = Checkin.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            -23.5505,
            -46.6333,
            10);
        checkin.Deactivate(DateTime.UtcNow);

        _checkinRepository
            .Setup(x => x.GetByIdAsync(checkin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkin);

        var result = await _handler.HandleAsync(new CancelCheckinCommand(checkin.Id));

        result.IsSuccess.Should().BeTrue();
        _checkinRepository.Verify(x => x.UpdateAsync(It.IsAny<Checkin>(), It.IsAny<CancellationToken>()), Times.Never);
        _realtimeNotifier.Verify(x => x.NotifyCheckinUndoneAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ActiveCheckin_ShouldDeactivateAndNotify()
    {
        var checkin = Checkin.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            -23.5505,
            -46.6333,
            10);

        _checkinRepository
            .Setup(x => x.GetByIdAsync(checkin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkin);

        _checkinRepository
            .Setup(x => x.CountActiveByGameDayAsync(checkin.GameDayId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await _handler.HandleAsync(new CancelCheckinCommand(checkin.Id));

        result.IsSuccess.Should().BeTrue();
        checkin.IsActive.Should().BeFalse();
        checkin.CancelledAtUtc.Should().NotBeNull();

        _checkinRepository.Verify(x => x.UpdateAsync(It.IsAny<Checkin>(), It.IsAny<CancellationToken>()), Times.Once);
        _checkinRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _realtimeNotifier.Verify(x => x.NotifyCheckinUndoneAsync(checkin.GameDayId, checkin.PlayerId, It.IsAny<CancellationToken>()), Times.Once);
        _realtimeNotifier.Verify(x => x.NotifyCheckinCountUpdatedAsync(checkin.GameDayId, 0, It.IsAny<CancellationToken>()), Times.Once);
    }
}
