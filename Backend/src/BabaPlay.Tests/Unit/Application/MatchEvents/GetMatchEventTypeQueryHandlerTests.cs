using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.MatchEvents;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.MatchEvents;

public class GetMatchEventTypeQueryHandlerTests
{
    private readonly Mock<IMatchEventTypeRepository> _typeRepository = new();
    private readonly GetMatchEventTypeQueryHandler _handler;

    public GetMatchEventTypeQueryHandlerTests()
    {
        _handler = new GetMatchEventTypeQueryHandler(_typeRepository.Object);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldReturnError()
    {
        _typeRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MatchEventType?)null);

        var result = await _handler.HandleAsync(new GetMatchEventTypeQuery(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_EVENT_TYPE_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_Existing_ShouldReturnResponse()
    {
        var type = MatchEventType.Create(Guid.NewGuid(), "goal", "Goal", 2, true);

        _typeRepository
            .Setup(x => x.GetByIdAsync(type.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(type);

        var result = await _handler.HandleAsync(new GetMatchEventTypeQuery(type.Id));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Code.Should().Be("goal");
    }
}
