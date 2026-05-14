using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Scores;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Scores;

public class GetAttendanceRankingQueryHandlerTests
{
    private readonly Mock<IPlayerScoreRepository> _repo = new();
    private readonly GetAttendanceRankingQueryHandler _handler;

    public GetAttendanceRankingQueryHandlerTests()
    {
        _handler = new GetAttendanceRankingQueryHandler(_repo.Object);
    }

    [Fact]
    public async Task Handle_WhenFromUtcIsAfterToUtc_ShouldReturnInvalidPeriod()
    {
        var query = new GetAttendanceRankingQuery(
            Page: 1,
            PageSize: 10,
            FromUtc: new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            ToUtc: new DateTime(2026, 1, 31, 23, 59, 59, DateTimeKind.Utc));

        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_PERIOD");
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldMapAttendancesAndRank()
    {
        var first = PlayerScore.Create(Guid.NewGuid(), Guid.NewGuid());
        first.ReplaceBreakdown(new(10, 2, 1, 4, 0, 0));

        var second = PlayerScore.Create(Guid.NewGuid(), Guid.NewGuid());
        second.ReplaceBreakdown(new(9, 2, 1, 4, 0, 0));

        _repo.Setup(r => r.GetAttendanceRankingAsync(null, 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync([first, second]);

        var result = await _handler.HandleAsync(new GetAttendanceRankingQuery(1, 10, null, null));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value![0].Rank.Should().Be(1);
        result.Value[0].AttendanceCount.Should().Be(10);
        result.Value[1].Rank.Should().Be(2);
        result.Value[1].AttendanceCount.Should().Be(9);
    }
}