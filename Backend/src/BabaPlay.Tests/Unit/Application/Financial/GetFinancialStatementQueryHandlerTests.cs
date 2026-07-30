using BabaPlay.Application.Queries.Financial;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Financial;

public class GetFinancialStatementQueryHandlerTests
{
    private readonly Mock<ICashTransactionRepository> _cashRepo = new();
    private readonly Mock<IPlayerRepository> _playerRepo = new();
    private readonly GetFinancialStatementQueryHandler _handler;

    public GetFinancialStatementQueryHandlerTests()
    {
        _handler = new GetFinancialStatementQueryHandler(_cashRepo.Object, _playerRepo.Object);
    }

    [Fact]
    public async Task Handle_InvalidMonth_ShouldReturnError()
    {
        var result = await _handler.HandleAsync(new GetFinancialStatementQuery(2026, 13));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_MONTH");
    }

    [Fact]
    public async Task Handle_ValidMonth_ShouldCalculateTotalsAndCategorize()
    {
        var tenantId = Guid.NewGuid();
        var playerId = Guid.NewGuid();

        var player = Player.Create(tenantId, Guid.NewGuid(), "Carlos Silva", "carlos@test.com", "71999998888", null);
        typeof(Player).GetProperty(nameof(Player.Id))!.SetValue(player, playerId);

        var incomeTx = CashTransaction.Create(tenantId, CashTransactionType.Income, 500m, new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc), "Mensalidade Maio", playerId);
        var expenseTx = CashTransaction.Create(tenantId, CashTransactionType.Expense, 200m, new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc), "Aluguel de Campo", null);

        _cashRepo.Setup(x => x.GetByPeriodAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CashTransaction> { incomeTx, expenseTx });

        _playerRepo.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Player> { player });

        var result = await _handler.HandleAsync(new GetFinancialStatementQuery(2026, 5));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.PeriodYear.Should().Be(2026);
        result.Value.PeriodMonth.Should().Be(5);
        result.Value.TotalIncomes.Should().Be(500m);
        result.Value.TotalExpenses.Should().Be(200m);
        result.Value.NetBalance.Should().Be(300m);
        result.Value.Items.Should().HaveCount(2);

        var incomeItem = result.Value.Items.First(x => x.Type == CashTransactionType.Income);
        incomeItem.Category.Should().Be("Mensalidade");
        incomeItem.PlayerName.Should().Be("Carlos Silva");

        var expenseItem = result.Value.Items.First(x => x.Type == CashTransactionType.Expense);
        expenseItem.Category.Should().Be("Aluguel de Campo");
        expenseItem.PlayerName.Should().BeNull();
    }
}
