using System;
using System.Threading;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Xunit;
using BabaPlay.Web.Components.Financial;
using BabaPlay.Web.Models;
using BabaPlay.Web.Services.Http;

namespace BabaPlay.Tests.Web.Financial;

public class PixPaymentModalTests : TestContext
{
    [Fact]
    public void PixPaymentModal_WhenNotOpen_ShouldRenderNothing()
    {
        // Arrange
        var mockService = new Mock<IFinancialApiService>();
        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<PixPaymentModal>(parameters => parameters
            .Add(p => p.IsOpen, false)
            .Add(p => p.Invoice, null));

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void PixPaymentModal_WhenOpen_ShouldRenderPixDetailsAndQrCode()
    {
        // Arrange
        var mockService = new Mock<IFinancialApiService>();
        var invoiceId = Guid.NewGuid();
        var invoice = new InvoiceDto
        {
            Id = invoiceId,
            Amount = 100m,
            OpenAmount = 100m,
            Month = 5,
            Year = 2026,
            DueDateUtc = new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Status = 0
        };

        var pixDetails = new PixPaymentDetailsDto
        {
            InvoiceId = invoiceId,
            Amount = 100m,
            QrCodeCopyAndPaste = "00020126580014br.gov.bcb.pix0136babaplay",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30),
            TxId = "PIX-12345",
            Status = "Pending"
        };

        mockService
            .Setup(x => x.GetPixPaymentAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pixDetails);

        Services.AddSingleton(mockService.Object);
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = RenderComponent<PixPaymentModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Invoice, invoice));

        // Assert
        cut.Markup.Should().Contain("Pagamento via Pix");
        cut.Markup.Should().Contain("100,00");
        cut.Markup.Should().Contain("00020126580014br.gov.bcb.pix0136babaplay");
        cut.Find("[data-testid='btn-copy-pix']").Should().NotBeNull();
    }

    [Fact]
    public async Task PixPaymentModal_ConfirmPayment_ShouldCallApiServiceAndFireEvent()
    {
        // Arrange
        var mockService = new Mock<IFinancialApiService>();
        var invoiceId = Guid.NewGuid();
        var invoice = new InvoiceDto
        {
            Id = invoiceId,
            Amount = 100m,
            OpenAmount = 100m,
            Month = 5,
            Year = 2026,
            DueDateUtc = new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Status = 0
        };

        var pixDetails = new PixPaymentDetailsDto
        {
            InvoiceId = invoiceId,
            Amount = 100m,
            QrCodeCopyAndPaste = "00020126580014br.gov.bcb.pix0136babaplay",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30),
            TxId = "PIX-12345",
            Status = "Pending"
        };

        mockService
            .Setup(x => x.GetPixPaymentAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pixDetails);

        mockService
            .Setup(x => x.ConfirmPixPaymentAsync(invoiceId, "PIX-12345", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Services.AddSingleton(mockService.Object);
        JSInterop.Mode = JSRuntimeMode.Loose;

        bool eventFired = false;

        // Act
        var cut = RenderComponent<PixPaymentModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Invoice, invoice)
            .Add(p => p.OnPaymentConfirmed, () => eventFired = true));

        var confirmBtn = cut.Find("[data-testid='btn-confirm-pix-payment']");
        await confirmBtn.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        mockService.Verify(x => x.ConfirmPixPaymentAsync(invoiceId, "PIX-12345", It.IsAny<CancellationToken>()), Times.Once);
        eventFired.Should().BeTrue();
        cut.Markup.Should().Contain("Pagamento Confirmado!");
    }
}
