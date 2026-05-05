using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Application.Commands.Financial;

public sealed class ReverseMonthlyFeePaymentCommandHandler
    : ICommandHandler<ReverseMonthlyFeePaymentCommand, Result<MonthlyFeePaymentResponse>>
{
    private readonly IMonthlyFeePaymentRepository _paymentRepository;
    private readonly IPlayerMonthlyFeeRepository _monthlyFeeRepository;
    private readonly ITenantContext _tenantContext;

    public ReverseMonthlyFeePaymentCommandHandler(
        IMonthlyFeePaymentRepository paymentRepository,
        IPlayerMonthlyFeeRepository monthlyFeeRepository,
        ITenantContext tenantContext)
    {
        _paymentRepository = paymentRepository;
        _monthlyFeeRepository = monthlyFeeRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<MonthlyFeePaymentResponse>> HandleAsync(ReverseMonthlyFeePaymentCommand cmd, CancellationToken ct = default)
    {
        if (cmd.PaymentId == Guid.Empty)
            return Result<MonthlyFeePaymentResponse>.Fail("FINANCIAL_INVALID_PAYMENT_ID", "PaymentId is required.");

        if (cmd.ReversedAtUtc.Kind != DateTimeKind.Utc)
            return Result<MonthlyFeePaymentResponse>.Fail("FINANCIAL_INVALID_REVERSED_AT", "ReversedAtUtc must be UTC.");

        var payment = await _paymentRepository.GetByIdAsync(cmd.PaymentId, ct);
        if (payment is null || payment.TenantId != _tenantContext.TenantId)
            return Result<MonthlyFeePaymentResponse>.Fail("MONTHLY_FEE_PAYMENT_NOT_FOUND", "Monthly fee payment was not found.");

        var monthlyFee = await _monthlyFeeRepository.GetByIdAsync(payment.MonthlyFeeId, ct);
        if (monthlyFee is null || monthlyFee.TenantId != _tenantContext.TenantId)
            return Result<MonthlyFeePaymentResponse>.Fail("MONTHLY_FEE_NOT_FOUND", "Monthly fee was not found.");

        try
        {
            payment.Reverse(cmd.ReversedAtUtc);
            monthlyFee.RevertPayment(payment.Amount, cmd.ReversedAtUtc);

            await _paymentRepository.UpdateAsync(payment, ct);
            await _monthlyFeeRepository.UpdateAsync(monthlyFee, ct);

            await _paymentRepository.SaveChangesAsync(ct);
            await _monthlyFeeRepository.SaveChangesAsync(ct);

            return Result<MonthlyFeePaymentResponse>.Ok(new MonthlyFeePaymentResponse(
                payment.Id,
                payment.TenantId,
                payment.MonthlyFeeId,
                payment.Amount,
                payment.PaidAtUtc,
                payment.Notes,
                payment.IsReversed,
                payment.ReversedAtUtc,
                payment.IsActive,
                payment.CreatedAt));
        }
        catch (ValidationException ex)
        {
            var firstError = ex.Errors.FirstOrDefault();
            var message = firstError.Value?.FirstOrDefault() ?? ex.Message;
            return Result<MonthlyFeePaymentResponse>.Fail("FINANCIAL_VALIDATION_ERROR", message);
        }
    }
}
