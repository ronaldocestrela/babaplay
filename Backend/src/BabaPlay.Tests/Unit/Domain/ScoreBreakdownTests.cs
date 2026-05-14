using BabaPlay.Domain.ValueObjects;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class ScoreBreakdownTests
{
    [Fact]
    public void CalculateTotal_ShouldUseOfficialWeights()
    {
        var breakdown = new ScoreBreakdown(
            AttendanceCount: 3,
            Wins: 2,
            Draws: 1,
            Goals: 4,
            YellowCards: 2,
            RedCards: 1);

        var total = breakdown.CalculateTotal();

        // (3*1) + (2*3) + (1*1) + (4*2) - (2*1) - (1*3) = 13
        total.Should().Be(13);
    }
}