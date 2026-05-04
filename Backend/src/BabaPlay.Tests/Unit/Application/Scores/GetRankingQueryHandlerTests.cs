using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Scores;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Scores;

public class GetRankingQueryHandlerTests
{
    private readonly Mock<IPlayerScoreRepository> _repo = new();
    private readonly GetRankingQueryHandler _handler;

    public GetRankingQueryHandlerTests()
    {
        _handler = new GetRankingQueryHandler(_repo.Object);
    }

    [Fact]
    public async Task Handle_WhenPeriodIsIncomplete_ShouldReturnInvalidPeriod()
    {
        var query = new GetRankingQuery(
            Page: 1,
            PageSize: 20,
            FromUtc: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ToUtc: null);

        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_PERIOD");
    }

    [Fact]
    public async Task Handle_WhenNoScores_ShouldReturnEmptyList()
    {
        _repo.Setup(r => r.GetRankingAsync(null, 0, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.HandleAsync(new GetRankingQuery(1, 20, null, null));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldMapAndSetRankUsingPageOffset()
    {
        var first = PlayerScore.Create(Guid.NewGuid(), Guid.NewGuid());
        first.ReplaceBreakdown(new(3, 1, 1, 2, 0, 0));

        var second = PlayerScore.Create(Guid.NewGuid(), Guid.NewGuid());
        second.ReplaceBreakdown(new(2, 1, 0, 1, 0, 0));

        _repo.Setup(r => r.GetRankingAsync(null, 20, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync([first, second]);

        var result = await _handler.HandleAsync(new GetRankingQuery(2, 20, null, null));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value![0].Rank.Should().Be(21);
        result.Value[1].Rank.Should().Be(22);
    }
}