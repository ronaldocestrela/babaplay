using BabaPlay.Application.Commands.Financial;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Financial;

public class RegisterMonthlyFeePaymentCommandHandlerTests
{
    private readonly Mock<IPlayerMonthlyFeeRepository> _monthlyFeeRepo = new();
    private readonly Mock<IMonthlyFeePaymentRepository> _paymentRepo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly RegisterMonthlyFeePaymentCommandHandler _handler;

    public RegisterMonthlyFeePaymentCommandHandlerTests()
    {
        _tenantContext.SetupGet(x => x.TenantId).Returns(Guid.NewGuid());
        _handler = new RegisterMonthlyFeePaymentCommandHandler(_monthlyFeeRepo.Object, _paymentRepo.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task Handle_MonthlyFeeNotFound_ShouldReturnNotFoundError()
    {
        var command = new RegisterMonthlyFeePaymentCommand(
            Guid.NewGuid(),
            50m,
            new DateTime(2026, 05, 09, 0, 0, 0, DateTimeKind.Utc),
            "PIX");

        _monthlyFeeRepo
            .Setup(x => x.GetByIdAsync(command.MonthlyFeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerMonthlyFee?)null);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("MONTHLY_FEE_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldRegisterPaymentAndUpdateMonthlyFee()
    {
        var tenantId = _tenantContext.Object.TenantId;
        var monthlyFee = PlayerMonthlyFee.Create(
            tenantId,
            Guid.NewGuid(),
            2026,
            5,
            150m,
            new DateTime(2026, 05, 10, 0, 0, 0, DateTimeKind.Utc),
            "Mensalidade maio");

        var command = new RegisterMonthlyFeePaymentCommand(
            monthlyFee.Id,
            150m,
            new DateTime(2026, 05, 09, 0, 0, 0, DateTimeKind.Utc),
            "PIX");

        _monthlyFeeRepo
            .Setup(x => x.GetByIdAsync(command.MonthlyFeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyFee);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.MonthlyFeeId.Should().Be(monthlyFee.Id);

        _paymentRepo.Verify(x => x.AddAsync(It.IsAny<MonthlyFeePayment>(), It.IsAny<CancellationToken>()), Times.Once);
        _paymentRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _monthlyFeeRepo.Verify(x => x.UpdateAsync(monthlyFee, It.IsAny<CancellationToken>()), Times.Once);
    }
}
