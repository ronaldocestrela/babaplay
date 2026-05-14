using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

public sealed class MonthlyFeePayment : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid MonthlyFeeId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime PaidAtUtc { get; private set; }
    public string? Notes { get; private set; }
    public bool IsReversed { get; private set; }
    public DateTime? ReversedAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    private MonthlyFeePayment() { }

    public static MonthlyFeePayment Create(
        Guid tenantId,
        Guid monthlyFeeId,
        decimal amount,
        DateTime paidAtUtc,
        string? notes)
    {
        ValidateCreate(tenantId, monthlyFeeId, amount, paidAtUtc);

        return new MonthlyFeePayment
        {
            TenantId = tenantId,
            MonthlyFeeId = monthlyFeeId,
            Amount = amount,
            PaidAtUtc = paidAtUtc,
            Notes = notes?.Trim(),
            IsReversed = false,
            ReversedAtUtc = null,
            IsActive = true,
        };
    }

    public void Reverse(DateTime reversedAtUtc)
    {
        if (reversedAtUtc.Kind != DateTimeKind.Utc)
            throw new ValidationException("ReversedAtUtc", "ReversedAtUtc must be UTC.");

        if (IsReversed)
            return;

        IsReversed = true;
        ReversedAtUtc = reversedAtUtc;
        MarkUpdated();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        MarkUpdated();
    }

    private static void ValidateCreate(
        Guid tenantId,
        Guid monthlyFeeId,
        decimal amount,
        DateTime paidAtUtc)
    {
        if (tenantId == Guid.Empty)
            throw new ValidationException("TenantId", "TenantId is required.");

        if (monthlyFeeId == Guid.Empty)
            throw new ValidationException("MonthlyFeeId", "MonthlyFeeId is required.");

        if (amount <= 0)
            throw new ValidationException("Amount", "Amount must be greater than zero.");

        if (paidAtUtc.Kind != DateTimeKind.Utc)
            throw new ValidationException("PaidAtUtc", "PaidAtUtc must be UTC.");
    }
}
