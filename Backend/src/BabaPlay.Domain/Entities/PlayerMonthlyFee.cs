using BabaPlay.Domain.Enums;
using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

public sealed class PlayerMonthlyFee : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid PlayerId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime DueDateUtc { get; private set; }
    public decimal PaidAmount { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }
    public MonthlyFeeStatus Status { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private PlayerMonthlyFee() { }

    public static PlayerMonthlyFee Create(
        Guid tenantId,
        Guid playerId,
        int year,
        int month,
        decimal amount,
        DateTime dueDateUtc,
        string? description)
    {
        ValidateCreate(tenantId, playerId, year, month, amount, dueDateUtc);

        return new PlayerMonthlyFee
        {
            TenantId = tenantId,
            PlayerId = playerId,
            Year = year,
            Month = month,
            Amount = amount,
            DueDateUtc = dueDateUtc,
            PaidAmount = 0,
            PaidAtUtc = null,
            Status = MonthlyFeeStatus.Open,
            Description = description?.Trim(),
            IsActive = true,
        };
    }

    public void ApplyPayment(decimal paymentAmount, DateTime paidAtUtc)
    {
        if (!IsActive)
            throw new ValidationException("MonthlyFee", "Monthly fee is inactive.");

        if (Status == MonthlyFeeStatus.Cancelled)
            throw new ValidationException("Status", "Cancelled monthly fee cannot receive payments.");

        if (paymentAmount <= 0)
            throw new ValidationException("PaymentAmount", "PaymentAmount must be greater than zero.");

        if (paidAtUtc.Kind != DateTimeKind.Utc)
            throw new ValidationException("PaidAtUtc", "PaidAtUtc must be UTC.");

        var newPaidAmount = PaidAmount + paymentAmount;

        if (newPaidAmount > Amount)
            throw new ValidationException("PaymentAmount", "Payment exceeds monthly fee amount.");

        PaidAmount = newPaidAmount;

        if (PaidAmount == Amount)
        {
            Status = MonthlyFeeStatus.Paid;
            PaidAtUtc = paidAtUtc;
        }

        MarkUpdated();
    }

    public void RevertPayment(decimal paymentAmount, DateTime referenceUtc)
    {
        if (!IsActive)
            throw new ValidationException("MonthlyFee", "Monthly fee is inactive.");

        if (paymentAmount <= 0)
            throw new ValidationException("PaymentAmount", "PaymentAmount must be greater than zero.");

        if (referenceUtc.Kind != DateTimeKind.Utc)
            throw new ValidationException("ReferenceUtc", "ReferenceUtc must be UTC.");

        if (paymentAmount > PaidAmount)
            throw new ValidationException("PaymentAmount", "Reversal exceeds paid amount.");

        PaidAmount -= paymentAmount;

        if (PaidAmount == 0)
            PaidAtUtc = null;

        if (PaidAmount == Amount)
        {
            Status = MonthlyFeeStatus.Paid;
        }
        else
        {
            Status = DueDateUtc < referenceUtc
                ? MonthlyFeeStatus.Overdue
                : MonthlyFeeStatus.Open;
        }

        MarkUpdated();
    }

    public void MarkOverdue(DateTime referenceUtc)
    {
        if (referenceUtc.Kind != DateTimeKind.Utc)
            throw new ValidationException("ReferenceUtc", "ReferenceUtc must be UTC.");

        if (Status != MonthlyFeeStatus.Open)
            return;

        if (DueDateUtc < referenceUtc)
        {
            Status = MonthlyFeeStatus.Overdue;
            MarkUpdated();
        }
    }

    public void Cancel()
    {
        if (Status == MonthlyFeeStatus.Cancelled)
            return;

        if (Status == MonthlyFeeStatus.Paid)
            throw new ValidationException("Status", "Paid monthly fee cannot be cancelled.");

        Status = MonthlyFeeStatus.Cancelled;
        IsActive = false;
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
        Guid playerId,
        int year,
        int month,
        decimal amount,
        DateTime dueDateUtc)
    {
        if (tenantId == Guid.Empty)
            throw new ValidationException("TenantId", "TenantId is required.");

        if (playerId == Guid.Empty)
            throw new ValidationException("PlayerId", "PlayerId is required.");

        if (year < 2000 || year > 2100)
            throw new ValidationException("Year", "Year must be between 2000 and 2100.");

        if (month < 1 || month > 12)
            throw new ValidationException("Month", "Month must be between 1 and 12.");

        if (amount <= 0)
            throw new ValidationException("Amount", "Amount must be greater than zero.");

        if (dueDateUtc.Kind != DateTimeKind.Utc)
            throw new ValidationException("DueDateUtc", "DueDateUtc must be UTC.");
    }
}
