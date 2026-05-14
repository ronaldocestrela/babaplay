using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using BabaPlay.Domain.Exceptions;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Domain;

public class CashTransactionTests
{
    [Fact]
    public void Create_ValidIncome_ShouldCreateActiveTransaction()
    {
        var occurredOnUtc = new DateTime(2026, 05, 01, 10, 0, 0, DateTimeKind.Utc);

        var transaction = CashTransaction.Create(
            Guid.NewGuid(),
            CashTransactionType.Income,
            120.50m,
            occurredOnUtc,
            "  Mensalidade recebida  ",
            Guid.NewGuid());

        transaction.Id.Should().NotBeEmpty();
        transaction.Type.Should().Be(CashTransactionType.Income);
        transaction.Amount.Should().Be(120.50m);
        transaction.OccurredOnUtc.Should().Be(occurredOnUtc);
        transaction.Description.Should().Be("Mensalidade recebida");
        transaction.IsActive.Should().BeTrue();
        transaction.SignedAmount.Should().Be(120.50m);
    }

    [Fact]
    public void Create_Expense_ShouldExposeNegativeSignedAmount()
    {
        var transaction = CashTransaction.Create(
            Guid.NewGuid(),
            CashTransactionType.Expense,
            80m,
            new DateTime(2026, 05, 01, 12, 0, 0, DateTimeKind.Utc),
            "Aluguel de campo",
            null);

        transaction.SignedAmount.Should().Be(-80m);
    }

    [Fact]
    public void Create_ZeroAmount_ShouldThrowValidationException()
    {
        var act = () => CashTransaction.Create(
            Guid.NewGuid(),
            CashTransactionType.Income,
            0m,
            new DateTime(2026, 05, 01, 10, 0, 0, DateTimeKind.Utc),
            "Mensalidade",
            null);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_LocalDate_ShouldThrowValidationException()
    {
        var act = () => CashTransaction.Create(
            Guid.NewGuid(),
            CashTransactionType.Income,
            10m,
            new DateTime(2026, 05, 01, 10, 0, 0, DateTimeKind.Local),
            "Mensalidade",
            null);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Deactivate_Twice_ShouldBeIdempotent()
    {
        var transaction = CashTransaction.Create(
            Guid.NewGuid(),
            CashTransactionType.Income,
            20m,
            new DateTime(2026, 05, 01, 10, 0, 0, DateTimeKind.Utc),
            "Mensalidade",
            null);

        transaction.Deactivate();

        var act = () => transaction.Deactivate();

        act.Should().NotThrow();
        transaction.IsActive.Should().BeFalse();
    }
}
