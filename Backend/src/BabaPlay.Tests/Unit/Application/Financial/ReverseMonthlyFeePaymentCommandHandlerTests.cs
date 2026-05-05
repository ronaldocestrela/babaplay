using BabaPlay.Application.Commands.Financial;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Financial;

public class ReverseMonthlyFeePaymentCommandHandlerTests
{
    private readonly Mock<IMonthlyFeePaymentRepository> _paymentRepo = new();
    private readonly Mock<IPlayerMonthlyFeeRepository> _monthlyFeeRepo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly ReverseMonthlyFeePaymentCommandHandler _handler;

    public ReverseMonthlyFeePaymentCommandHandlerTests()
    {
        _tenantContext.SetupGet(x => x.TenantId).Returns(Guid.NewGuid());
        _handler = new ReverseMonthlyFeePaymentCommandHandler(
            _paymentRepo.Object,
            _monthlyFeeRepo.Object,
            _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_EmptyPaymentId_ShouldReturnValidationError()
    {
        var command = new ReverseMonthlyFeePaymentCommand(Guid.Empty, DateTime.UtcNow);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("FINANCIAL_INVALID_PAYMENT_ID");
    }

    [Fact]
    public async Task Handle_TenantNotResolved_ShouldReturnValidationError()
    {
        _tenantContext.SetupGet(x => x.TenantId).Returns(Guid.Empty);
        var command = new ReverseMonthlyFeePaymentCommand(Guid.NewGuid(), DateTime.UtcNow);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TENANT_NOT_RESOLVED");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldReverseAndPersist()
    {
        var tenantId = _tenantContext.Object.TenantId;
        var playerId = Guid.NewGuid();
        var paidAt = new DateTime(2026, 5, 10, 10, 0, 0, DateTimeKind.Utc);

        var monthlyFee = PlayerMonthlyFee.Create(
            tenantId,
            playerId,
            2026,
            5,
            100m,
            new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc),
            "Mensalidade");

        monthlyFee.ApplyPayment(100m, paidAt);

        var payment = MonthlyFeePayment.Create(
            tenantId,
            monthlyFee.Id,
            100m,
            paidAt,
            "Pagamento");

        _paymentRepo.Setup(x => x.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _monthlyFeeRepo.Setup(x => x.GetByIdAsync(monthlyFee.Id, It.IsAny<CancellationToken>())).ReturnsAsync(monthlyFee);

        var result = await _handler.HandleAsync(new ReverseMonthlyFeePaymentCommand(payment.Id, DateTime.UtcNow));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.IsReversed.Should().BeTrue();

        _paymentRepo.Verify(x => x.UpdateAsync(payment, It.IsAny<CancellationToken>()), Times.Once);
        _monthlyFeeRepo.Verify(x => x.UpdateAsync(monthlyFee, It.IsAny<CancellationToken>()), Times.Once);
    }
}
