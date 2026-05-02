using BabaPlay.Application.Commands.Positions;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Positions;

public class UpdatePositionCommandHandlerTests
{
    private readonly Mock<IPositionRepository> _positionRepo = new();
    private readonly UpdatePositionCommandHandler _handler;

    public UpdatePositionCommandHandlerTests()
    {
        _handler = new UpdatePositionCommandHandler(_positionRepo.Object);
    }

    [Fact]
    public async Task Handle_PositionNotFound_ShouldReturnPositionNotFound()
    {
        var id = Guid.NewGuid();
        _positionRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Position?)null);

        var result = await _handler.HandleAsync(new UpdatePositionCommand(id, "GK", "Goleiro", null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("POSITION_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_DuplicateCode_ShouldReturnPositionAlreadyExists()
    {
        var position = Position.Create(Guid.NewGuid(), "GK", "Goleiro", null);
        _positionRepo.Setup(r => r.GetByIdAsync(position.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);
        _positionRepo.Setup(r => r.ExistsByNormalizedCodeAsync("CM", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(new UpdatePositionCommand(position.Id, "cm", "Meia", null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("POSITION_ALREADY_EXISTS");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdatePosition()
    {
        var position = Position.Create(Guid.NewGuid(), "GK", "Goleiro", null);
        _positionRepo.Setup(r => r.GetByIdAsync(position.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);
        _positionRepo.Setup(r => r.ExistsByNormalizedCodeAsync("CM", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(new UpdatePositionCommand(position.Id, "cm", "Meia", "Centro"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Code.Should().Be("cm");
        result.Value.Name.Should().Be("Meia");
        _positionRepo.Verify(r => r.UpdateAsync(It.IsAny<Position>(), It.IsAny<CancellationToken>()), Times.Once);
        _positionRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
