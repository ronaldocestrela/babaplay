using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Commands.Financial;

public sealed class GeneratePixPaymentCommandHandler : ICommandHandler<GeneratePixPaymentCommand, Result<PixPaymentDetailsResponse>>
{
    private readonly IPlayerMonthlyFeeRepository _monthlyFeeRepository;

    public GeneratePixPaymentCommandHandler(IPlayerMonthlyFeeRepository monthlyFeeRepository)
    {
        _monthlyFeeRepository = monthlyFeeRepository;
    }

    public async Task<Result<PixPaymentDetailsResponse>> HandleAsync(GeneratePixPaymentCommand command, CancellationToken ct = default)
    {
        var fee = await _monthlyFeeRepository.GetByIdAsync(command.InvoiceId, ct);
        if (fee is null)
            return Result<PixPaymentDetailsResponse>.Fail("MONTHLY_FEE_NOT_FOUND", "Monthly fee not found.");

        if (fee.Status == MonthlyFeeStatus.Paid)
            return Result<PixPaymentDetailsResponse>.Fail("INVOICE_ALREADY_PAID", "Monthly fee is already paid.");

        var openAmount = fee.Amount - fee.PaidAmount;
        var txId = $"PIX-{fee.Id:N}".Substring(0, 24);
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(30);

        // EMV Co Pix String payload simulation
        var amountStr = openAmount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        var copyAndPaste = $"00020126580014br.gov.bcb.pix0136babaplay-{fee.Id:N}520400005303986540{amountStr.Length:D2}{amountStr}5802BR5916BABA PLAY SAAS6009SALVADOR62070503***6304ABCD";

        var response = new PixPaymentDetailsResponse(
            fee.Id,
            openAmount,
            copyAndPaste,
            expiresAtUtc,
            txId,
            "Pending");

        return Result<PixPaymentDetailsResponse>.Ok(response);
    }
}
