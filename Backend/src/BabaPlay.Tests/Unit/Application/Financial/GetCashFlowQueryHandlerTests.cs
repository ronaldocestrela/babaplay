using BabaPlay.Application.Queries.Financial;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Financial;

public class GetCashFlowQueryHandlerTests
{
    private readonly Mock<ICashTransactionRepository> _repo = new();
    private readonly GetCashFlowQueryHandler _handler;

    public GetCashFlowQueryHandlerTests()
    {
        _handler = new GetCashFlowQueryHandler(_repo.Object);
    }

    [Fact]
    public async Task Handle_InvalidPeriod_ShouldReturnValidationError()
    {
        var fromUtc = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = await _handler.HandleAsync(new GetCashFlowQuery(fromUtc, toUtc));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_PERIOD");
    }

    [Fact]
    public async Task Handle_ValidPeriod_ShouldReturnAggregatedCashFlow()
    {
        var tenantId = Guid.NewGuid();
        var fromUtc = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2026, 5, 31, 23, 59, 59, DateTimeKind.Utc);

        var income = CashTransaction.Create(tenantId, CashTransactionType.Income, 200m, fromUtc.AddDays(1), "Entrada", null);
        var expense = CashTransaction.Create(tenantId, CashTransactionType.Expense, 50m, fromUtc.AddDays(2), "Saida", null);

        _repo.Setup(x => x.GetByPeriodAsync(fromUtc, toUtc, It.IsAny<CancellationToken>()))
            .ReturnsAsync([income, expense]);

        var result = await _handler.HandleAsync(new GetCashFlowQuery(fromUtc, toUtc));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalIncome.Should().Be(200m);
        result.Value.TotalExpense.Should().Be(50m);
        result.Value.Balance.Should().Be(150m);
        result.Value.Entries.Should().HaveCount(2);
    }
}
