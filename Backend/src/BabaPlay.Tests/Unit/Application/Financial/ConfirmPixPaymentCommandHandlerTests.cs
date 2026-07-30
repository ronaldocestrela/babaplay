using BabaPlay.Application.Commands.Financial;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Financial;

public class ConfirmPixPaymentCommandHandlerTests
{
    private readonly Mock<IPlayerMonthlyFeeRepository> _monthlyFeeRepo = new();
    private readonly Mock<ICashTransactionRepository> _cashRepo = new();
    private readonly ConfirmPixPaymentCommandHandler _handler;

    public ConfirmPixPaymentCommandHandlerTests()
    {
        _handler = new ConfirmPixPaymentCommandHandler(_monthlyFeeRepo.Object, _cashRepo.Object);
    }

    [Fact]
    public async Task Handle_InvoiceNotFound_ShouldReturnError()
    {
        var result = await _handler.HandleAsync(new ConfirmPixPaymentCommand(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MONTHLY_FEE_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_InvoiceAlreadyPaid_ShouldReturnError()
    {
        var tenantId = Guid.NewGuid();
        var fee = PlayerMonthlyFee.Create(tenantId, Guid.NewGuid(), 2026, 5, 100m, DateTime.UtcNow, null);
        fee.ApplyPayment(100m, DateTime.UtcNow);

        _monthlyFeeRepo.Setup(x => x.GetByIdAsync(fee.Id, It.IsAny<CancellationToken>())).ReturnsAsync(fee);

        var result = await _handler.HandleAsync(new ConfirmPixPaymentCommand(fee.Id));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVOICE_ALREADY_PAID");
    }

    [Fact]
    public async Task Handle_ValidPendingInvoice_ShouldPayInvoiceAndAddCashIncome()
    {
        var tenantId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var fee = PlayerMonthlyFee.Create(tenantId, playerId, 2026, 5, 120m, DateTime.UtcNow.AddDays(5), "Mensalidade Maio");

        _monthlyFeeRepo.Setup(x => x.GetByIdAsync(fee.Id, It.IsAny<CancellationToken>())).ReturnsAsync(fee);

        var result = await _handler.HandleAsync(new ConfirmPixPaymentCommand(fee.Id, "TX123"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Amount.Should().Be(120m);
        fee.Status.Should().Be(MonthlyFeeStatus.Paid);

        _monthlyFeeRepo.Verify(x => x.UpdateAsync(fee, It.IsAny<CancellationToken>()), Times.Once);
        _cashRepo.Verify(x => x.AddAsync(It.Is<CashTransaction>(c => c.Type == CashTransactionType.Income && c.Amount == 120m), It.IsAny<CancellationToken>()), Times.Once);
    }
}
