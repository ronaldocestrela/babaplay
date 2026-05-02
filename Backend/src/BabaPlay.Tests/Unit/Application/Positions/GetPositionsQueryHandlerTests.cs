using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Positions;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Positions;

public class GetPositionsQueryHandlerTests
{
    private readonly Mock<IPositionRepository> _positionRepo = new();
    private readonly GetPositionsQueryHandler _handler;

    public GetPositionsQueryHandlerTests()
    {
        _handler = new GetPositionsQueryHandler(_positionRepo.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedList()
    {
        var tenantId = Guid.NewGuid();
        _positionRepo.Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Position>
            {
                Position.Create(tenantId, "GK", "Goleiro", null),
                Position.Create(tenantId, "CM", "Meia", null),
            });

        var result = await _handler.HandleAsync(new GetPositionsQuery());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }
}
