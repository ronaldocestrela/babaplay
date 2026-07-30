using BabaPlay.Application.Commands.Financial;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Financial;

public class GeneratePixPaymentCommandHandlerTests
{
    private readonly Mock<IPlayerMonthlyFeeRepository> _monthlyFeeRepo = new();
    private readonly GeneratePixPaymentCommandHandler _handler;

    public GeneratePixPaymentCommandHandlerTests()
    {
        _handler = new GeneratePixPaymentCommandHandler(_monthlyFeeRepo.Object);
    }

    [Fact]
    public async Task Handle_InvoiceNotFound_ShouldReturnError()
    {
        var result = await _handler.HandleAsync(new GeneratePixPaymentCommand(Guid.NewGuid()));

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

        var result = await _handler.HandleAsync(new GeneratePixPaymentCommand(fee.Id));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVOICE_ALREADY_PAID");
    }

    [Fact]
    public async Task Handle_ValidInvoice_ShouldReturnPixDetails()
    {
        var tenantId = Guid.NewGuid();
        var fee = PlayerMonthlyFee.Create(tenantId, Guid.NewGuid(), 2026, 5, 80m, DateTime.UtcNow.AddDays(5), "Mensalidade Maio");

        _monthlyFeeRepo.Setup(x => x.GetByIdAsync(fee.Id, It.IsAny<CancellationToken>())).ReturnsAsync(fee);

        var result = await _handler.HandleAsync(new GeneratePixPaymentCommand(fee.Id));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Amount.Should().Be(80m);
        result.Value.QrCodeCopyAndPaste.Should().Contain("babaplay");
        result.Value.TxId.Should().StartWith("PIX-");
        result.Value.Status.Should().Be("Pending");
    }
}
