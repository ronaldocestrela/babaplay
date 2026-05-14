using BabaPlay.Application.Commands.MatchEvents;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.MatchEvents;

public class UpdateMatchEventTypeCommandHandlerTests
{
    private readonly Mock<IMatchEventTypeRepository> _typeRepository = new();
    private readonly UpdateMatchEventTypeCommandHandler _handler;

    public UpdateMatchEventTypeCommandHandlerTests()
    {
        _handler = new UpdateMatchEventTypeCommandHandler(_typeRepository.Object);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldReturnNotFound()
    {
        _typeRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MatchEventType?)null);

        var result = await _handler.HandleAsync(new UpdateMatchEventTypeCommand(Guid.NewGuid(), "goal", "Goal", 2));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_EVENT_TYPE_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_DuplicateCode_ShouldReturnConflict()
    {
        var type = MatchEventType.Create(Guid.NewGuid(), "goal", "Goal", 2, true);

        _typeRepository
            .Setup(x => x.GetByIdAsync(type.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(type);
        _typeRepository
            .Setup(x => x.ExistsByNormalizedCodeAsync("ASSIST", type.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(new UpdateMatchEventTypeCommand(type.Id, "assist", "Assist", 1));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_EVENT_TYPE_ALREADY_EXISTS");
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldUpdateType()
    {
        var type = MatchEventType.Create(Guid.NewGuid(), "goal", "Goal", 2, true);

        _typeRepository
            .Setup(x => x.GetByIdAsync(type.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(type);
        _typeRepository
            .Setup(x => x.ExistsByNormalizedCodeAsync("ASSIST", type.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(new UpdateMatchEventTypeCommand(type.Id, "assist", "Assist", 1));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Code.Should().Be("assist");
        _typeRepository.Verify(x => x.UpdateAsync(type, It.IsAny<CancellationToken>()), Times.Once);
        _typeRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
