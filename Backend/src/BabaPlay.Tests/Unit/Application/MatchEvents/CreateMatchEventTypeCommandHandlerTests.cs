using BabaPlay.Application.Commands.MatchEvents;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.MatchEvents;

public class CreateMatchEventTypeCommandHandlerTests
{
    private readonly Mock<IMatchEventTypeRepository> _typeRepository = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly CreateMatchEventTypeCommandHandler _handler;

    public CreateMatchEventTypeCommandHandlerTests()
    {
        _tenantContext.SetupGet(x => x.TenantId).Returns(Guid.NewGuid());
        _handler = new CreateMatchEventTypeCommandHandler(_typeRepository.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_DuplicateCode_ShouldReturnConflictError()
    {
        _typeRepository
            .Setup(x => x.ExistsByNormalizedCodeAsync("GOAL", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(new CreateMatchEventTypeCommand("goal", "Goal", 2, false));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MATCH_EVENT_TYPE_ALREADY_EXISTS");
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateType()
    {
        _typeRepository
            .Setup(x => x.ExistsByNormalizedCodeAsync("ASSIST", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(new CreateMatchEventTypeCommand("assist", "Assist", 1, false));

        result.IsSuccess.Should().BeTrue();
        _typeRepository.Verify(x => x.AddAsync(It.IsAny<MatchEventType>(), It.IsAny<CancellationToken>()), Times.Once);
        _typeRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
