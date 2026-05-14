using BabaPlay.Domain.Exceptions;
using BabaPlay.Domain.ValueObjects;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class BillingCompetenceTests
{
    [Fact]
    public void Create_ValidCompetence_ShouldReturnValueObject()
    {
        var competence = BillingCompetence.Create(2026, 5);

        competence.Year.Should().Be(2026);
        competence.Month.Should().Be(5);
        competence.Display.Should().Be("2026-05");
    }

    [Fact]
    public void Create_InvalidMonth_ShouldThrowValidationException()
    {
        var act = () => BillingCompetence.Create(2026, 13);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void FromDateUtc_ShouldMapYearAndMonth()
    {
        var date = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        var competence = BillingCompetence.FromDateUtc(date);

        competence.Should().Be(BillingCompetence.Create(2026, 12));
    }
}
