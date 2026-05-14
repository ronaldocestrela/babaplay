using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using BabaPlay.Domain.ValueObjects;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class PlayerScoreTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateActiveScoreWithZeroedCounters()
    {
        var tenantId = Guid.NewGuid();
        var playerId = Guid.NewGuid();

        var score = PlayerScore.Create(tenantId, playerId);

        score.Id.Should().NotBeEmpty();
        score.TenantId.Should().Be(tenantId);
        score.PlayerId.Should().Be(playerId);
        score.AttendanceCount.Should().Be(0);
        score.Wins.Should().Be(0);
        score.Draws.Should().Be(0);
        score.Goals.Should().Be(0);
        score.YellowCards.Should().Be(0);
        score.RedCards.Should().Be(0);
        score.ScoreTotal.Should().Be(0);
        score.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_EmptyTenantId_ShouldThrowValidationException()
    {
        var act = () => PlayerScore.Create(Guid.Empty, Guid.NewGuid());

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_EmptyPlayerId_ShouldThrowValidationException()
    {
        var act = () => PlayerScore.Create(Guid.NewGuid(), Guid.Empty);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void ApplyDelta_WithPositiveValues_ShouldRecomputeScore()
    {
        var score = PlayerScore.Create(Guid.NewGuid(), Guid.NewGuid());

        score.ApplyDelta(new ScoreBreakdown(
            AttendanceCount: 4,
            Wins: 2,
            Draws: 1,
            Goals: 3,
            YellowCards: 1,
            RedCards: 1));

        // (4*1) + (2*3) + (1*1) + (3*2) - (1*1) - (1*3) = 13
        score.ScoreTotal.Should().Be(13);
    }

    [Fact]
    public void ApplyDelta_WithRollback_ShouldBeIdempotentByState()
    {
        var score = PlayerScore.Create(Guid.NewGuid(), Guid.NewGuid());
        var baseline = score.GetBreakdown();

        score.ApplyDelta(new ScoreBreakdown(1, 1, 0, 2, 1, 0));
        score.ApplyDelta(new ScoreBreakdown(-1, -1, 0, -2, -1, 0));

        score.GetBreakdown().Should().Be(baseline);
        score.ScoreTotal.Should().Be(0);
    }

    [Fact]
    public void ApplyDelta_WhenResultWouldBeNegativeCounter_ShouldThrowValidationException()
    {
        var score = PlayerScore.Create(Guid.NewGuid(), Guid.NewGuid());

        var act = () => score.ApplyDelta(new ScoreBreakdown(-1, 0, 0, 0, 0, 0));

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void ReplaceBreakdown_ShouldSetExactValuesAndRecomputeTotal()
    {
        var score = PlayerScore.Create(Guid.NewGuid(), Guid.NewGuid());

        score.ReplaceBreakdown(new ScoreBreakdown(10, 5, 2, 8, 3, 1));

        score.AttendanceCount.Should().Be(10);
        score.Wins.Should().Be(5);
        score.Draws.Should().Be(2);
        score.Goals.Should().Be(8);
        score.YellowCards.Should().Be(3);
        score.RedCards.Should().Be(1);
        score.ScoreTotal.Should().Be(37);
    }
}