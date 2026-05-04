using BabaPlay.Application.Commands.GameDays;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.GameDays;

public class UpdateGameDayCommandHandlerTests
{
    private readonly Mock<IGameDayRepository> _gameDayRepo = new();
    private readonly UpdateGameDayCommandHandler _handler;

    public UpdateGameDayCommandHandlerTests()
    {
        _handler = new UpdateGameDayCommandHandler(_gameDayRepo.Object);
    }

    [Fact]
    public async Task Handle_GameDayNotFound_ShouldReturnGameDayNotFound()
    {
        var id = Guid.NewGuid();
        _gameDayRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameDay?)null);

        var result = await _handler.HandleAsync(new UpdateGameDayCommand(id, "Rodada", DateTime.UtcNow.AddHours(2), null, null, 22));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("GAMEDAY_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_DuplicateNameAndSchedule_ShouldReturnAlreadyExists()
    {
        var gameDay = GameDay.Create(Guid.NewGuid(), "Rodada", DateTime.UtcNow.AddHours(1), null, null, 22);
        var newScheduledAt = DateTime.UtcNow.AddHours(2);
        _gameDayRepo.Setup(r => r.GetByIdAsync(gameDay.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gameDay);
        _gameDayRepo.Setup(r => r.ExistsByNormalizedNameAndScheduledAtAsync("RODADA NOVA", newScheduledAt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(new UpdateGameDayCommand(gameDay.Id, "Rodada Nova", newScheduledAt, null, null, 22));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("GAMEDAY_ALREADY_EXISTS");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateGameDay()
    {
        var gameDay = GameDay.Create(Guid.NewGuid(), "Rodada", DateTime.UtcNow.AddHours(1), null, null, 22);
        var newScheduledAt = DateTime.UtcNow.AddHours(3);
        _gameDayRepo.Setup(r => r.GetByIdAsync(gameDay.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gameDay);
        _gameDayRepo.Setup(r => r.ExistsByNormalizedNameAndScheduledAtAsync("RODADA", newScheduledAt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(new UpdateGameDayCommand(gameDay.Id, "Rodada", newScheduledAt, "Campo B", null, 20));

        result.IsSuccess.Should().BeTrue();
        result.Value!.MaxPlayers.Should().Be(20);
        _gameDayRepo.Verify(r => r.UpdateAsync(It.IsAny<GameDay>(), It.IsAny<CancellationToken>()), Times.Once);
        _gameDayRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
