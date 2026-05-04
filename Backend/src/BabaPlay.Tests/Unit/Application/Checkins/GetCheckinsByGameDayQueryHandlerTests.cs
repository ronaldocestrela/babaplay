using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Checkins;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Checkins;

public class GetCheckinsByGameDayQueryHandlerTests
{
    private readonly Mock<ICheckinRepository> _checkinRepository = new();
    private readonly GetCheckinsByGameDayQueryHandler _handler;

    public GetCheckinsByGameDayQueryHandlerTests()
    {
        _handler = new GetCheckinsByGameDayQueryHandler(_checkinRepository.Object);
    }

    [Fact]
    public async Task Handle_NoCheckins_ShouldReturnEmptyList()
    {
        var gameDayId = Guid.NewGuid();
        _checkinRepository
            .Setup(x => x.GetActiveByGameDayAsync(gameDayId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.HandleAsync(new GetCheckinsByGameDayQuery(gameDayId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithCheckins_ShouldReturnMappedList()
    {
        var tenantId = Guid.NewGuid();
        var gameDayId = Guid.NewGuid();
        var checkins = new List<Checkin>
        {
            Checkin.Create(tenantId, Guid.NewGuid(), gameDayId, DateTime.UtcNow, -23.5505, -46.6333, 5),
            Checkin.Create(tenantId, Guid.NewGuid(), gameDayId, DateTime.UtcNow.AddMinutes(-3), -23.5506, -46.6332, 8),
        };

        _checkinRepository
            .Setup(x => x.GetActiveByGameDayAsync(gameDayId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkins);

        var result = await _handler.HandleAsync(new GetCheckinsByGameDayQuery(gameDayId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value!.Select(c => c.GameDayId).Should().OnlyContain(id => id == gameDayId);
    }
}
