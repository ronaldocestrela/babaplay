using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;
using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Application.Commands.Financial;

public sealed class RegisterMonthlyFeePaymentCommandHandler
    : ICommandHandler<RegisterMonthlyFeePaymentCommand, Result<MonthlyFeePaymentResponse>>
{
    private readonly IPlayerMonthlyFeeRepository _monthlyFeeRepository;
    private readonly IMonthlyFeePaymentRepository _paymentRepository;
    private readonly ITenantContext _tenantContext;

    public RegisterMonthlyFeePaymentCommandHandler(
        IPlayerMonthlyFeeRepository monthlyFeeRepository,
        IMonthlyFeePaymentRepository paymentRepository,
        ITenantContext tenantContext)
    {
        _monthlyFeeRepository = monthlyFeeRepository;
        _paymentRepository = paymentRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<MonthlyFeePaymentResponse>> HandleAsync(RegisterMonthlyFeePaymentCommand cmd, CancellationToken ct = default)
    {
        if (cmd.MonthlyFeeId == Guid.Empty)
            return Result<MonthlyFeePaymentResponse>.Fail("FINANCIAL_INVALID_MONTHLY_FEE_ID", "MonthlyFeeId is required.");

        if (cmd.Amount <= 0)
            return Result<MonthlyFeePaymentResponse>.Fail("FINANCIAL_INVALID_PAYMENT_AMOUNT", "Amount must be greater than zero.");

        if (cmd.PaidAtUtc.Kind != DateTimeKind.Utc)
            return Result<MonthlyFeePaymentResponse>.Fail("FINANCIAL_INVALID_PAID_AT", "PaidAtUtc must be UTC.");

        if (_tenantContext.TenantId == Guid.Empty)
            return Result<MonthlyFeePaymentResponse>.Fail("TENANT_NOT_RESOLVED", "Tenant context is required.");

        var monthlyFee = await _monthlyFeeRepository.GetByIdAsync(cmd.MonthlyFeeId, ct);
        if (monthlyFee is null || monthlyFee.TenantId != _tenantContext.TenantId)
            return Result<MonthlyFeePaymentResponse>.Fail("MONTHLY_FEE_NOT_FOUND", "Monthly fee was not found.");

        try
        {
            monthlyFee.ApplyPayment(cmd.Amount, cmd.PaidAtUtc);
            var payment = MonthlyFeePayment.Create(
                _tenantContext.TenantId,
                monthlyFee.Id,
                cmd.Amount,
                cmd.PaidAtUtc,
                cmd.Notes);

            await _paymentRepository.AddAsync(payment, ct);
            await _monthlyFeeRepository.UpdateAsync(monthlyFee, ct);

            await _paymentRepository.SaveChangesAsync(ct);
            await _monthlyFeeRepository.SaveChangesAsync(ct);

            return Result<MonthlyFeePaymentResponse>.Ok(ToResponse(payment));
        }
        catch (ValidationException ex)
        {
            var firstError = ex.Errors.FirstOrDefault();
            var message = firstError.Value?.FirstOrDefault() ?? ex.Message;
            return Result<MonthlyFeePaymentResponse>.Fail("FINANCIAL_VALIDATION_ERROR", message);
        }
    }

    private static MonthlyFeePaymentResponse ToResponse(MonthlyFeePayment payment) => new(
        payment.Id,
        payment.TenantId,
        payment.MonthlyFeeId,
        payment.Amount,
        payment.PaidAtUtc,
        payment.Notes,
        payment.IsReversed,
        payment.ReversedAtUtc,
        payment.IsActive,
        payment.CreatedAt);
}
