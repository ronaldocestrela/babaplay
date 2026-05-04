using BabaPlay.Application.Commands.GameDays;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.GameDays;

public class DeleteGameDayCommandHandlerTests
{
    private readonly Mock<IGameDayRepository> _gameDayRepo = new();
    private readonly DeleteGameDayCommandHandler _handler;

    public DeleteGameDayCommandHandlerTests()
    {
        _handler = new DeleteGameDayCommandHandler(_gameDayRepo.Object);
    }

    [Fact]
    public async Task Handle_GameDayNotFound_ShouldReturnGameDayNotFound()
    {
        _gameDayRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameDay?)null);

        var result = await _handler.HandleAsync(new DeleteGameDayCommand(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("GAMEDAY_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ExistingGameDay_ShouldDeactivateAndReturnSuccess()
    {
        var gameDay = GameDay.Create(Guid.NewGuid(), "Rodada", DateTime.UtcNow.AddHours(2), null, null, 22);
        _gameDayRepo.Setup(r => r.GetByIdAsync(gameDay.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gameDay);

        var result = await _handler.HandleAsync(new DeleteGameDayCommand(gameDay.Id));

        result.IsSuccess.Should().BeTrue();
        gameDay.IsActive.Should().BeFalse();
        _gameDayRepo.Verify(r => r.UpdateAsync(It.IsAny<GameDay>(), It.IsAny<CancellationToken>()), Times.Once);
        _gameDayRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
