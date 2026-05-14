using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Checkins;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Checkins;

public class GetCheckinsByPlayerQueryHandlerTests
{
    private readonly Mock<ICheckinRepository> _checkinRepository = new();
    private readonly GetCheckinsByPlayerQueryHandler _handler;

    public GetCheckinsByPlayerQueryHandlerTests()
    {
        _handler = new GetCheckinsByPlayerQueryHandler(_checkinRepository.Object);
    }

    [Fact]
    public async Task Handle_NoCheckins_ShouldReturnEmptyList()
    {
        var playerId = Guid.NewGuid();
        _checkinRepository
            .Setup(x => x.GetActiveByPlayerAsync(playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.HandleAsync(new GetCheckinsByPlayerQuery(playerId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithCheckins_ShouldReturnMappedList()
    {
        var tenantId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var checkins = new List<Checkin>
        {
            Checkin.Create(tenantId, playerId, Guid.NewGuid(), DateTime.UtcNow, -23.5505, -46.6333, 5),
            Checkin.Create(tenantId, playerId, Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-4), -23.5506, -46.6332, 7),
        };

        _checkinRepository
            .Setup(x => x.GetActiveByPlayerAsync(playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkins);

        var result = await _handler.HandleAsync(new GetCheckinsByPlayerQuery(playerId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value!.Select(c => c.PlayerId).Should().OnlyContain(id => id == playerId);
    }
}
