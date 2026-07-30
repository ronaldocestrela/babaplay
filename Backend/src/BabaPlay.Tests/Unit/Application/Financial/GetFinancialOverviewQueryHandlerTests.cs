using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Financial;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Financial;

public class GetFinancialOverviewQueryHandlerTests
{
    private readonly Mock<ICashTransactionRepository> _cashRepo = new();
    private readonly Mock<IPlayerMonthlyFeeRepository> _monthlyFeeRepo = new();
    private readonly GetFinancialOverviewQueryHandler _handler;

    public GetFinancialOverviewQueryHandlerTests()
    {
        _handler = new GetFinancialOverviewQueryHandler(_cashRepo.Object, _monthlyFeeRepo.Object);
    }

    [Fact]
    public async Task Handle_InvalidYear_ShouldReturnValidationError()
    {
        var result = await _handler.HandleAsync(new GetFinancialOverviewQuery(0, 5));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_COMPETENCE");
    }

    [Fact]
    public async Task Handle_InvalidMonth_ShouldReturnValidationError()
    {
        var result = await _handler.HandleAsync(new GetFinancialOverviewQuery(2026, 13));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_COMPETENCE");
    }

    [Fact]
    public async Task Handle_ValidCompetence_ShouldReturnConsolidatedOverview()
    {
        var tenantId = Guid.NewGuid();
        var playerId1 = Guid.NewGuid();
        var playerId2 = Guid.NewGuid();

        var fee1 = PlayerMonthlyFee.Create(tenantId, playerId1, 2026, 5, 100m, new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc), null);
        fee1.ApplyPayment(100m, new DateTime(2026, 5, 8, 0, 0, 0, DateTimeKind.Utc));

        var fee2 = PlayerMonthlyFee.Create(tenantId, playerId2, 2026, 5, 50m, new DateTime(2026, 5, 12, 0, 0, 0, DateTimeKind.Utc), null);

        var overdueFee = PlayerMonthlyFee.Create(tenantId, playerId2, 2026, 4, 100m, new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc), null);

        var income = CashTransaction.Create(tenantId, CashTransactionType.Income, 250m, new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc), "Mensalidades", null);
        var expense = CashTransaction.Create(tenantId, CashTransactionType.Expense, 50m, new DateTime(2026, 5, 5, 0, 0, 0, DateTimeKind.Utc), "Bolas", null);

        _monthlyFeeRepo.Setup(x => x.GetByCompetenceAsync(2026, 5, It.IsAny<CancellationToken>())).ReturnsAsync([fee1, fee2]);
        _monthlyFeeRepo.Setup(x => x.GetOverdueAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync([overdueFee]);
        _cashRepo.Setup(x => x.GetByPeriodAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync([income, expense]);

        var result = await _handler.HandleAsync(new GetFinancialOverviewQuery(2026, 5));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Year.Should().Be(2026);
        result.Value.Month.Should().Be(5);
        result.Value.TotalIncome.Should().Be(250m);
        result.Value.TotalExpense.Should().Be(50m);
        result.Value.NetBalance.Should().Be(200m);
        result.Value.PendingMonthlyFeesAmount.Should().Be(100m);
        result.Value.DelinquentPlayersCount.Should().Be(1);
        result.Value.CollectionRatePercentage.Should().Be(66.67m);
        result.Value.RecentTransactions.Should().HaveCount(2);
    }
}
