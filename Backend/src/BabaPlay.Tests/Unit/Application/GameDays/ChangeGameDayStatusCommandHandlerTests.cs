using BabaPlay.Application.Commands.GameDays;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.GameDays;

public class ChangeGameDayStatusCommandHandlerTests
{
    private readonly Mock<IGameDayRepository> _gameDayRepo = new();
    private readonly ChangeGameDayStatusCommandHandler _handler;

    public ChangeGameDayStatusCommandHandlerTests()
    {
        _handler = new ChangeGameDayStatusCommandHandler(_gameDayRepo.Object);
    }

    [Fact]
    public async Task Handle_GameDayNotFound_ShouldReturnGameDayNotFound()
    {
        _gameDayRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameDay?)null);

        var result = await _handler.HandleAsync(new ChangeGameDayStatusCommand(Guid.NewGuid(), GameDayStatus.Confirmed));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("GAMEDAY_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ValidTransition_ShouldUpdateStatus()
    {
        var gameDay = GameDay.Create(Guid.NewGuid(), "Rodada", DateTime.UtcNow.AddHours(2), null, null, 22);
        _gameDayRepo.Setup(r => r.GetByIdAsync(gameDay.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gameDay);

        var result = await _handler.HandleAsync(new ChangeGameDayStatusCommand(gameDay.Id, GameDayStatus.Confirmed));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(GameDayStatus.Confirmed);
        _gameDayRepo.Verify(r => r.UpdateAsync(It.IsAny<GameDay>(), It.IsAny<CancellationToken>()), Times.Once);
        _gameDayRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
