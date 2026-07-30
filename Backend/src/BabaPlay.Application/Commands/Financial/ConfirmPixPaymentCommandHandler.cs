using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.Commands.Financial;

public sealed class ConfirmPixPaymentCommandHandler : ICommandHandler<ConfirmPixPaymentCommand, Result<MonthlyFeePaymentResponse>>
{
    private readonly IPlayerMonthlyFeeRepository _monthlyFeeRepository;
    private readonly ICashTransactionRepository _cashTransactionRepository;

    public ConfirmPixPaymentCommandHandler(
        IPlayerMonthlyFeeRepository monthlyFeeRepository,
        ICashTransactionRepository cashTransactionRepository)
    {
        _monthlyFeeRepository = monthlyFeeRepository;
        _cashTransactionRepository = cashTransactionRepository;
    }

    public async Task<Result<MonthlyFeePaymentResponse>> HandleAsync(ConfirmPixPaymentCommand command, CancellationToken ct = default)
    {
        var fee = await _monthlyFeeRepository.GetByIdAsync(command.InvoiceId, ct);
        if (fee is null)
            return Result<MonthlyFeePaymentResponse>.Fail("MONTHLY_FEE_NOT_FOUND", "Monthly fee not found.");

        if (fee.Status == MonthlyFeeStatus.Paid)
            return Result<MonthlyFeePaymentResponse>.Fail("INVOICE_ALREADY_PAID", "Monthly fee is already paid.");

        var openAmount = fee.Amount - fee.PaidAmount;
        if (openAmount <= 0)
            return Result<MonthlyFeePaymentResponse>.Fail("INVOICE_NO_OPEN_AMOUNT", "Monthly fee has no open amount to pay.");

        var now = DateTime.UtcNow;
        fee.ApplyPayment(openAmount, now);

        var cashTransaction = CashTransaction.Create(
            fee.TenantId,
            CashTransactionType.Income,
            openAmount,
            now,
            $"Pagamento Pix - Competência {fee.Month:D2}/{fee.Year}",
            fee.PlayerId);

        await _monthlyFeeRepository.UpdateAsync(fee, ct);
        await _cashTransactionRepository.AddAsync(cashTransaction, ct);

        var response = new MonthlyFeePaymentResponse(
            Guid.NewGuid(),
            fee.TenantId,
            fee.Id,
            openAmount,
            now,
            "Pagamento Pix efetuado com sucesso",
            false,
            null,
            true,
            now);

        return Result<MonthlyFeePaymentResponse>.Ok(response);
    }
}
