using BabaPlay.Application.Commands.GameDays;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.GameDays;

public class CreateGameDayCommandHandlerTests
{
    private readonly Mock<IGameDayRepository> _gameDayRepo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly CreateGameDayCommandHandler _handler;

    public CreateGameDayCommandHandlerTests()
    {
        _tenantContext.SetupGet(x => x.TenantId).Returns(Guid.NewGuid());
        _handler = new CreateGameDayCommandHandler(_gameDayRepo.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_EmptyName_ShouldReturnInvalidName()
    {
        var result = await _handler.HandleAsync(new CreateGameDayCommand("", DateTime.UtcNow.AddHours(1), null, null, 22));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_NAME");
    }

    [Fact]
    public async Task Handle_DuplicateNameAndSchedule_ShouldReturnAlreadyExists()
    {
        var scheduledAt = DateTime.UtcNow.AddHours(2);
        _gameDayRepo
            .Setup(r => r.ExistsByNormalizedNameAndScheduledAtAsync("RODADA", scheduledAt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(new CreateGameDayCommand("Rodada", scheduledAt, null, null, 22));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("GAMEDAY_ALREADY_EXISTS");
        _gameDayRepo.Verify(r => r.AddAsync(It.IsAny<GameDay>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateGameDay()
    {
        var scheduledAt = DateTime.UtcNow.AddHours(2);
        _gameDayRepo
            .Setup(r => r.ExistsByNormalizedNameAndScheduledAtAsync("RODADA", scheduledAt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(new CreateGameDayCommand("Rodada", scheduledAt, "Campo A", null, 18));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Rodada");
        result.Value.MaxPlayers.Should().Be(18);
        _gameDayRepo.Verify(r => r.AddAsync(It.IsAny<GameDay>(), It.IsAny<CancellationToken>()), Times.Once);
        _gameDayRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
