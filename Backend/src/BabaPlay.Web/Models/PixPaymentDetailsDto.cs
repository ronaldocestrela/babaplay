using System;

namespace BabaPlay.Web.Models;

public sealed record PixPaymentDetailsDto
{
    public Guid InvoiceId { get; init; }
    public decimal Amount { get; init; }
    public string QrCodeCopyAndPaste { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
    public string TxId { get; init; } = string.Empty;
    public string Status { get; init; } = "Pending";
}
