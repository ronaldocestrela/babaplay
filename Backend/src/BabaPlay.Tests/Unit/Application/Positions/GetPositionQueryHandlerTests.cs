using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Positions;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Positions;

public class GetPositionQueryHandlerTests
{
    private readonly Mock<IPositionRepository> _positionRepo = new();
    private readonly GetPositionQueryHandler _handler;

    public GetPositionQueryHandlerTests()
    {
        _handler = new GetPositionQueryHandler(_positionRepo.Object);
    }

    [Fact]
    public async Task Handle_PositionNotFound_ShouldReturnPositionNotFound()
    {
        _positionRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Position?)null);

        var result = await _handler.HandleAsync(new GetPositionQuery(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("POSITION_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ExistingActivePosition_ShouldReturnResponse()
    {
        var position = Position.Create(Guid.NewGuid(), "GK", "Goleiro", null);
        _positionRepo.Setup(r => r.GetByIdAsync(position.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);

        var result = await _handler.HandleAsync(new GetPositionQuery(position.Id));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(position.Id);
    }
}
