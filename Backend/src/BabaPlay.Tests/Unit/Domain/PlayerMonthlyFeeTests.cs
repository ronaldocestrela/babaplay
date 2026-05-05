using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class PlayerMonthlyFeeTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateOpenMonthlyFee()
    {
        var dueDateUtc = new DateTime(2026, 05, 10, 0, 0, 0, DateTimeKind.Utc);

        var fee = PlayerMonthlyFee.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            2026,
            5,
            150m,
            dueDateUtc,
            "Mensalidade maio");

        fee.Year.Should().Be(2026);
        fee.Month.Should().Be(5);
        fee.Amount.Should().Be(150m);
        fee.PaidAmount.Should().Be(0m);
        fee.Status.Should().Be(MonthlyFeeStatus.Open);
        fee.DueDateUtc.Should().Be(dueDateUtc);
        fee.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_InvalidMonth_ShouldThrowValidationException()
    {
        var act = () => PlayerMonthlyFee.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            2026,
            13,
            150m,
            new DateTime(2026, 05, 10, 0, 0, 0, DateTimeKind.Utc),
            "Mensalidade");

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void ApplyPayment_Partial_ShouldRemainOpen()
    {
        var fee = PlayerMonthlyFee.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            2026,
            5,
            150m,
            new DateTime(2026, 05, 10, 0, 0, 0, DateTimeKind.Utc),
            "Mensalidade");

        fee.ApplyPayment(50m, new DateTime(2026, 05, 08, 0, 0, 0, DateTimeKind.Utc));

        fee.PaidAmount.Should().Be(50m);
        fee.Status.Should().Be(MonthlyFeeStatus.Open);
        fee.PaidAtUtc.Should().BeNull();
    }

    [Fact]
    public void ApplyPayment_FullAmount_ShouldSetPaid()
    {
        var fee = PlayerMonthlyFee.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            2026,
            5,
            150m,
            new DateTime(2026, 05, 10, 0, 0, 0, DateTimeKind.Utc),
            "Mensalidade");

        var paidAtUtc = new DateTime(2026, 05, 09, 0, 0, 0, DateTimeKind.Utc);
        fee.ApplyPayment(150m, paidAtUtc);

        fee.PaidAmount.Should().Be(150m);
        fee.Status.Should().Be(MonthlyFeeStatus.Paid);
        fee.PaidAtUtc.Should().Be(paidAtUtc);
    }

    [Fact]
    public void ApplyPayment_OverAmount_ShouldThrowValidationException()
    {
        var fee = PlayerMonthlyFee.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            2026,
            5,
            150m,
            new DateTime(2026, 05, 10, 0, 0, 0, DateTimeKind.Utc),
            "Mensalidade");

        var act = () => fee.ApplyPayment(151m, new DateTime(2026, 05, 09, 0, 0, 0, DateTimeKind.Utc));

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void MarkOverdue_WhenPastDueAndOpen_ShouldSetOverdue()
    {
        var fee = PlayerMonthlyFee.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            2026,
            5,
            150m,
            new DateTime(2026, 05, 10, 0, 0, 0, DateTimeKind.Utc),
            "Mensalidade");

        fee.MarkOverdue(new DateTime(2026, 05, 11, 0, 0, 0, DateTimeKind.Utc));

        fee.Status.Should().Be(MonthlyFeeStatus.Overdue);
    }

    [Fact]
    public void Cancel_PaidMonthlyFee_ShouldThrowValidationException()
    {
        var fee = PlayerMonthlyFee.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            2026,
            5,
            150m,
            new DateTime(2026, 05, 10, 0, 0, 0, DateTimeKind.Utc),
            "Mensalidade");

        fee.ApplyPayment(150m, new DateTime(2026, 05, 09, 0, 0, 0, DateTimeKind.Utc));

        var act = () => fee.Cancel();

        act.Should().Throw<ValidationException>();
    }
}
