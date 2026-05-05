using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class MonthlyFeePaymentTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateActivePayment()
    {
        var paidAtUtc = new DateTime(2026, 05, 08, 0, 0, 0, DateTimeKind.Utc);

        var payment = MonthlyFeePayment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            90m,
            paidAtUtc,
            "PIX");

        payment.Amount.Should().Be(90m);
        payment.PaidAtUtc.Should().Be(paidAtUtc);
        payment.Notes.Should().Be("PIX");
        payment.IsReversed.Should().BeFalse();
        payment.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_NonPositiveAmount_ShouldThrowValidationException()
    {
        var act = () => MonthlyFeePayment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            0m,
            new DateTime(2026, 05, 08, 0, 0, 0, DateTimeKind.Utc),
            null);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Reverse_Twice_ShouldBeIdempotent()
    {
        var payment = MonthlyFeePayment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            120m,
            new DateTime(2026, 05, 08, 0, 0, 0, DateTimeKind.Utc),
            null);

        var reversedAtUtc = new DateTime(2026, 05, 12, 0, 0, 0, DateTimeKind.Utc);
        payment.Reverse(reversedAtUtc);

        var act = () => payment.Reverse(reversedAtUtc.AddDays(1));

        act.Should().NotThrow();
        payment.IsReversed.Should().BeTrue();
        payment.ReversedAtUtc.Should().Be(reversedAtUtc);
    }
}
