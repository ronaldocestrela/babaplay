using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.MatchEvents;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.MatchEvents;

public class GetMatchEventTypesQueryHandlerTests
{
    private readonly Mock<IMatchEventTypeRepository> _typeRepository = new();
    private readonly GetMatchEventTypesQueryHandler _handler;

    public GetMatchEventTypesQueryHandlerTests()
    {
        _handler = new GetMatchEventTypesQueryHandler(_typeRepository.Object);
    }

    [Fact]
    public async Task Handle_EmptyList_ShouldReturnEmpty()
    {
        _typeRepository
            .Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.HandleAsync(new GetMatchEventTypesQuery());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithData_ShouldReturnMappedItems()
    {
        var first = MatchEventType.Create(Guid.NewGuid(), "goal", "Goal", 2, true);
        var second = MatchEventType.Create(Guid.NewGuid(), "yellow_card", "Yellow Card", -1, true);

        _typeRepository
            .Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([first, second]);

        var result = await _handler.HandleAsync(new GetMatchEventTypesQuery());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }
}
