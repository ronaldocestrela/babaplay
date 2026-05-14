using BabaPlay.Application.Services;
using BabaPlay.Domain.ValueObjects;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Application.Scores;

public class ScoreComputationServiceTests
{
    private readonly ScoreComputationService _service = new();

    [Fact]
    public void ComputeTotal_ShouldReturnZero_ForEmptyBreakdown()
    {
        var result = _service.ComputeTotal(ScoreBreakdown.Zero);

        result.Should().Be(0);
    }

    [Fact]
    public void ComputeTotal_ShouldMatchOfficialFormula()
    {
        var breakdown = new ScoreBreakdown(
            AttendanceCount: 5,
            Wins: 3,
            Draws: 1,
            Goals: 6,
            YellowCards: 2,
            RedCards: 1);

        var result = _service.ComputeTotal(breakdown);

        // (5*1) + (3*3) + (1*1) + (6*2) - (2*1) - (1*3) = 22
        result.Should().Be(22);
    }
}