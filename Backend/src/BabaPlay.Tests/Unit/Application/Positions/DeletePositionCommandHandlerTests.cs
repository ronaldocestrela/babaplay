using BabaPlay.Application.Commands.Positions;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Positions;

public class DeletePositionCommandHandlerTests
{
    private readonly Mock<IPositionRepository> _positionRepo = new();
    private readonly DeletePositionCommandHandler _handler;

    public DeletePositionCommandHandlerTests()
    {
        _handler = new DeletePositionCommandHandler(_positionRepo.Object);
    }

    [Fact]
    public async Task Handle_PositionNotFound_ShouldReturnPositionNotFound()
    {
        _positionRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Position?)null);

        var result = await _handler.HandleAsync(new DeletePositionCommand(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("POSITION_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ExistingPosition_ShouldDeactivateAndReturnSuccess()
    {
        var position = Position.Create(Guid.NewGuid(), "GK", "Goleiro", null);
        _positionRepo.Setup(r => r.GetByIdAsync(position.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);
        _positionRepo.Setup(r => r.IsInUseAsync(position.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(new DeletePositionCommand(position.Id));

        result.IsSuccess.Should().BeTrue();
        position.IsActive.Should().BeFalse();
        _positionRepo.Verify(r => r.UpdateAsync(It.IsAny<Position>(), It.IsAny<CancellationToken>()), Times.Once);
        _positionRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PositionInUse_ShouldReturnConflict()
    {
        var position = Position.Create(Guid.NewGuid(), "CM", "Meia", null);
        _positionRepo.Setup(r => r.GetByIdAsync(position.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);
        _positionRepo.Setup(r => r.IsInUseAsync(position.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(new DeletePositionCommand(position.Id));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("POSITION_IN_USE");
        position.IsActive.Should().BeTrue();
        _positionRepo.Verify(r => r.UpdateAsync(It.IsAny<Position>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
