using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.GameDays;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.GameDays;

public class GetGameDayQueryHandlerTests
{
    private readonly Mock<IGameDayRepository> _gameDayRepo = new();
    private readonly GetGameDayQueryHandler _handler;

    public GetGameDayQueryHandlerTests()
    {
        _handler = new GetGameDayQueryHandler(_gameDayRepo.Object);
    }

    [Fact]
    public async Task Handle_GameDayNotFound_ShouldReturnGameDayNotFound()
    {
        _gameDayRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameDay?)null);

        var result = await _handler.HandleAsync(new GetGameDayQuery(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("GAMEDAY_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ExistingGameDay_ShouldReturnResponse()
    {
        var gameDay = GameDay.Create(Guid.NewGuid(), "Rodada", DateTime.UtcNow.AddHours(1), null, null, 22);
        _gameDayRepo.Setup(r => r.GetByIdAsync(gameDay.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gameDay);

        var result = await _handler.HandleAsync(new GetGameDayQuery(gameDay.Id));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(gameDay.Id);
    }
}
