using BabaPlay.Application.Commands.Scores;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Scores;

public class ApplyScoreDeltaCommandHandlerTests
{
    private readonly Mock<IPlayerScoreRepository> _repo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly ApplyScoreDeltaCommandHandler _handler;

    public ApplyScoreDeltaCommandHandlerTests()
    {
        _tenantContext.SetupGet(x => x.TenantId).Returns(Guid.NewGuid());
        _handler = new ApplyScoreDeltaCommandHandler(_repo.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_WhenSourceEventAlreadyProcessed_ShouldReturnDuplicateError()
    {
        var command = BuildValidCommand();

        _repo.Setup(r => r.HasProcessedSourceEventAsync(command.SourceEventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("DUPLICATE_SCORE_EVENT");
    }

    [Fact]
    public async Task Handle_WhenScoreDoesNotExistAndDeltaIsNegative_ShouldReturnPlayerScoreNotFound()
    {
        var command = BuildValidCommand() with { AttendanceDelta = -1 };

        _repo.Setup(r => r.HasProcessedSourceEventAsync(command.SourceEventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repo.Setup(r => r.GetByPlayerIdAsync(command.PlayerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerScore?)null);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("PLAYER_SCORE_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_WhenScoreDoesNotExistAndDeltaIsPositive_ShouldCreateScoreAndRegisterSourceEvent()
    {
        var command = BuildValidCommand() with { WinsDelta = 1, GoalsDelta = 2 };

        _repo.Setup(r => r.HasProcessedSourceEventAsync(command.SourceEventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repo.Setup(r => r.GetByPlayerIdAsync(command.PlayerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerScore?)null);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.AddAsync(It.IsAny<PlayerScore>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<PlayerScore>(), It.IsAny<CancellationToken>()), Times.Never);
        _repo.Verify(r => r.AddProcessedSourceEventAsync(It.IsAny<PlayerScoreSourceEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenScoreExists_ShouldApplyDeltaAndUpdate()
    {
        var command = BuildValidCommand() with { AttendanceDelta = 1, DrawsDelta = 1 };
        var existing = PlayerScore.Create(_tenantContext.Object.TenantId, command.PlayerId);

        _repo.Setup(r => r.HasProcessedSourceEventAsync(command.SourceEventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repo.Setup(r => r.GetByPlayerIdAsync(command.PlayerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.AddProcessedSourceEventAsync(It.IsAny<PlayerScoreSourceEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenResultingCountersBecomeNegative_ShouldReturnInvalidScoreDelta()
    {
        var command = BuildValidCommand() with { RedCardsDelta = -1 };
        var existing = PlayerScore.Create(_tenantContext.Object.TenantId, command.PlayerId);

        _repo.Setup(r => r.HasProcessedSourceEventAsync(command.SourceEventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repo.Setup(r => r.GetByPlayerIdAsync(command.PlayerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_SCORE_DELTA");
    }

    private static ApplyScoreDeltaCommand BuildValidCommand()
        => new(
            SourceEventId: Guid.NewGuid(),
            PlayerId: Guid.NewGuid(),
            AttendanceDelta: 1,
            WinsDelta: 0,
            DrawsDelta: 0,
            GoalsDelta: 0,
            YellowCardsDelta: 0,
            RedCardsDelta: 0);
}