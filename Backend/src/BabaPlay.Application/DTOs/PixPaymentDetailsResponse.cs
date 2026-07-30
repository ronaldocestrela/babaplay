namespace BabaPlay.Application.DTOs;

public sealed record PixPaymentDetailsResponse(
    Guid InvoiceId,
    decimal Amount,
    string QrCodeCopyAndPaste,
    DateTime ExpiresAtUtc,
    string TxId,
    string Status);
