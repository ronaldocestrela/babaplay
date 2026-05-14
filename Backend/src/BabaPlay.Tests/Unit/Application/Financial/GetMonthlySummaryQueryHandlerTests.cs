using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Financial;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Financial;

public class GetMonthlySummaryQueryHandlerTests
{
    private readonly Mock<IPlayerMonthlyFeeRepository> _monthlyFeeRepo = new();
    private readonly Mock<ICashTransactionRepository> _cashRepo = new();
    private readonly GetMonthlySummaryQueryHandler _handler;

    public GetMonthlySummaryQueryHandlerTests()
    {
        _handler = new GetMonthlySummaryQueryHandler(_monthlyFeeRepo.Object, _cashRepo.Object);
    }

    [Fact]
    public async Task Handle_InvalidYear_ShouldReturnValidationError()
    {
        var result = await _handler.HandleAsync(new GetMonthlySummaryQuery(0, 5));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_COMPETENCE");
    }

    [Fact]
    public async Task Handle_InvalidMonth_ShouldReturnValidationError()
    {
        var result = await _handler.HandleAsync(new GetMonthlySummaryQuery(2026, 13));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_COMPETENCE");
    }

    [Fact]
    public async Task Handle_ValidCompetence_ShouldReturnConsolidatedSummary()
    {
        var tenantId = Guid.NewGuid();

        var fee1 = PlayerMonthlyFee.Create(tenantId, Guid.NewGuid(), 2026, 5, 100m, new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc), null);
        fee1.ApplyPayment(100m, new DateTime(2026, 5, 8, 0, 0, 0, DateTimeKind.Utc));

        var fee2 = PlayerMonthlyFee.Create(tenantId, Guid.NewGuid(), 2026, 5, 50m, new DateTime(2026, 5, 12, 0, 0, 0, DateTimeKind.Utc), null);

        var income = CashTransaction.Create(tenantId, CashTransactionType.Income, 220m, new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc), "Entrou", null);
        var expense = CashTransaction.Create(tenantId, CashTransactionType.Expense, 40m, new DateTime(2026, 5, 5, 0, 0, 0, DateTimeKind.Utc), "Saiu", null);

        _monthlyFeeRepo.Setup(x => x.GetByCompetenceAsync(2026, 5, It.IsAny<CancellationToken>())).ReturnsAsync([fee1, fee2]);
        _cashRepo.Setup(x => x.GetByPeriodAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync([income, expense]);

        var result = await _handler.HandleAsync(new GetMonthlySummaryQuery(2026, 5));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.MonthlyFeesAmount.Should().Be(150m);
        result.Value.MonthlyFeesPaidAmount.Should().Be(100m);
        result.Value.MonthlyFeesOpenAmount.Should().Be(50m);
        result.Value.CashBalance.Should().Be(180m);
    }
}
