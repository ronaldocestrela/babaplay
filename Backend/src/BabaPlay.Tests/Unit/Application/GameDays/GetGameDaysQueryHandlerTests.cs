using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.GameDays;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.GameDays;

public class GetGameDaysQueryHandlerTests
{
    private readonly Mock<IGameDayRepository> _gameDayRepo = new();
    private readonly GetGameDaysQueryHandler _handler;

    public GetGameDaysQueryHandlerTests()
    {
        _handler = new GetGameDaysQueryHandler(_gameDayRepo.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedList()
    {
        var tenantId = Guid.NewGuid();
        _gameDayRepo.Setup(r => r.GetAllActiveAsync(It.IsAny<GameDayStatus?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameDay>
            {
                GameDay.Create(tenantId, "Rodada 1", DateTime.UtcNow.AddHours(1), null, null, 22),
                GameDay.Create(tenantId, "Rodada 2", DateTime.UtcNow.AddHours(2), null, null, 22),
            });

        var result = await _handler.HandleAsync(new GetGameDaysQuery(null));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }
}
