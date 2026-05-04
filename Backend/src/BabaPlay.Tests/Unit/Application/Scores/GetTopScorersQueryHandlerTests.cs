using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Scores;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Scores;

public class GetTopScorersQueryHandlerTests
{
    private readonly Mock<IPlayerScoreRepository> _repo = new();
    private readonly GetTopScorersQueryHandler _handler;

    public GetTopScorersQueryHandlerTests()
    {
        _handler = new GetTopScorersQueryHandler(_repo.Object);
    }

    [Fact]
    public async Task Handle_WhenPeriodIsInvalid_ShouldReturnInvalidPeriod()
    {
        var query = new GetTopScorersQuery(
            Page: 1,
            PageSize: 10,
            FromUtc: null,
            ToUtc: new DateTime(2026, 1, 31, 23, 59, 59, DateTimeKind.Utc));

        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_PERIOD");
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldMapGoalsAndRank()
    {
        var first = PlayerScore.Create(Guid.NewGuid(), Guid.NewGuid());
        first.ReplaceBreakdown(new(3, 1, 1, 7, 0, 0));

        var second = PlayerScore.Create(Guid.NewGuid(), Guid.NewGuid());
        second.ReplaceBreakdown(new(3, 1, 1, 6, 0, 0));

        _repo.Setup(r => r.GetTopScorersAsync(null, 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync([first, second]);

        var result = await _handler.HandleAsync(new GetTopScorersQuery(1, 10, null, null));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value![0].Rank.Should().Be(1);
        result.Value[0].Goals.Should().Be(7);
        result.Value[1].Rank.Should().Be(2);
        result.Value[1].Goals.Should().Be(6);
    }
}