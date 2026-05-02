using BabaPlay.Application.Commands.Positions;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Positions;

public class CreatePositionCommandHandlerTests
{
    private readonly Mock<IPositionRepository> _positionRepo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly CreatePositionCommandHandler _handler;

    public CreatePositionCommandHandlerTests()
    {
        _tenantContext.SetupGet(x => x.TenantId).Returns(Guid.NewGuid());
        _handler = new CreatePositionCommandHandler(_positionRepo.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_EmptyCode_ShouldReturnInvalidCode()
    {
        var result = await _handler.HandleAsync(new CreatePositionCommand("", "Goleiro", null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_CODE");
    }

    [Fact]
    public async Task Handle_DuplicateCode_ShouldReturnPositionAlreadyExists()
    {
        _positionRepo
            .Setup(r => r.ExistsByNormalizedCodeAsync("GK", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(new CreatePositionCommand("gk", "Goleiro", null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("POSITION_ALREADY_EXISTS");
        _positionRepo.Verify(r => r.AddAsync(It.IsAny<Position>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreatePosition()
    {
        _positionRepo
            .Setup(r => r.ExistsByNormalizedCodeAsync("GK", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(new CreatePositionCommand("gk", "Goleiro", "Defesa"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Code.Should().Be("gk");
        result.Value.Name.Should().Be("Goleiro");
        _positionRepo.Verify(r => r.AddAsync(It.IsAny<Position>(), It.IsAny<CancellationToken>()), Times.Once);
        _positionRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
