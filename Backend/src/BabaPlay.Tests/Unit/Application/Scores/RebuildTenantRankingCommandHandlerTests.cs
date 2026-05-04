using BabaPlay.Application.Commands.Scores;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Scores;

public class RebuildTenantRankingCommandHandlerTests
{
    private readonly Mock<IPlayerScoreRepository> _repo = new();
    private readonly RebuildTenantRankingCommandHandler _handler;

    public RebuildTenantRankingCommandHandlerTests()
    {
        _handler = new RebuildTenantRankingCommandHandler(_repo.Object);
    }

    [Fact]
    public async Task Handle_WhenPeriodIsIncomplete_ShouldReturnInvalidPeriod()
    {
        var command = new RebuildTenantRankingCommand(
            FromUtc: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ToUtc: null);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_PERIOD");
    }

    [Fact]
    public async Task Handle_WhenNoScores_ShouldReturnSuccessWithZeroProcessed()
    {
        _repo.Setup(r => r.GetAllActiveForRebuildAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.HandleAsync(new RebuildTenantRankingCommand(null, null));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ProcessedCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenScoresExist_ShouldRecomputeAndUpdateAll()
    {
        var scoreA = PlayerScore.Create(Guid.NewGuid(), Guid.NewGuid());
        scoreA.ReplaceBreakdown(new(3, 1, 1, 2, 0, 0));

        var scoreB = PlayerScore.Create(Guid.NewGuid(), Guid.NewGuid());
        scoreB.ReplaceBreakdown(new(2, 1, 0, 1, 0, 0));

        _repo.Setup(r => r.GetAllActiveForRebuildAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([scoreA, scoreB]);

        var result = await _handler.HandleAsync(new RebuildTenantRankingCommand(null, null));

        result.IsSuccess.Should().BeTrue();
        result.Value!.ProcessedCount.Should().Be(2);
        _repo.Verify(r => r.UpdateAsync(scoreA, It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateAsync(scoreB, It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnRebuildFailed()
    {
        _repo.Setup(r => r.GetAllActiveForRebuildAsync(null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db failure"));

        var result = await _handler.HandleAsync(new RebuildTenantRankingCommand(null, null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("RANKING_REBUILD_FAILED");
    }
}