using BabaPlay.Domain.Enums;
using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

public sealed class CashTransaction : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid? PlayerId { get; private set; }
    public CashTransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime OccurredOnUtc { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public decimal SignedAmount => Type == CashTransactionType.Expense ? -Amount : Amount;

    private CashTransaction() { }

    public static CashTransaction Create(
        Guid tenantId,
        CashTransactionType type,
        decimal amount,
        DateTime occurredOnUtc,
        string description,
        Guid? playerId)
    {
        Validate(tenantId, type, amount, occurredOnUtc, description, playerId);

        return new CashTransaction
        {
            TenantId = tenantId,
            PlayerId = playerId,
            Type = type,
            Amount = amount,
            OccurredOnUtc = occurredOnUtc,
            Description = description.Trim(),
            IsActive = true,
        };
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        MarkUpdated();
    }

    private static void Validate(
        Guid tenantId,
        CashTransactionType type,
        decimal amount,
        DateTime occurredOnUtc,
        string description,
        Guid? playerId)
    {
        if (tenantId == Guid.Empty)
            throw new ValidationException("TenantId", "TenantId is required.");

        if (!Enum.IsDefined(type))
            throw new ValidationException("Type", "Type is invalid.");

        if (amount <= 0)
            throw new ValidationException("Amount", "Amount must be greater than zero.");

        if (occurredOnUtc.Kind != DateTimeKind.Utc)
            throw new ValidationException("OccurredOnUtc", "OccurredOnUtc must be UTC.");

        if (string.IsNullOrWhiteSpace(description))
            throw new ValidationException("Description", "Description is required.");

        if (playerId.HasValue && playerId.Value == Guid.Empty)
            throw new ValidationException("PlayerId", "PlayerId cannot be empty.");
    }
}
