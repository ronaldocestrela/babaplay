using BabaPlay.Domain.Exceptions;
using BabaPlay.Domain.ValueObjects;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class RankingPeriodTests
{
    [Fact]
    public void Create_ValidRange_ShouldReturnPeriod()
    {
        var fromUtc = new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2026, 01, 31, 23, 59, 59, DateTimeKind.Utc);

        var period = RankingPeriod.Create(fromUtc, toUtc);

        period.FromUtc.Should().Be(fromUtc);
        period.ToUtc.Should().Be(toUtc);
    }

    [Fact]
    public void Create_FromGreaterThanTo_ShouldThrowValidationException()
    {
        var fromUtc = new DateTime(2026, 02, 01, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2026, 01, 31, 23, 59, 59, DateTimeKind.Utc);

        var act = () => RankingPeriod.Create(fromUtc, toUtc);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_NonUtcDates_ShouldThrowValidationException()
    {
        var fromLocal = new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Local);
        var toUtc = new DateTime(2026, 01, 31, 23, 59, 59, DateTimeKind.Utc);

        var act = () => RankingPeriod.Create(fromLocal, toUtc);

        act.Should().Throw<ValidationException>();
    }
}