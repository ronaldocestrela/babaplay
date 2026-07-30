using System;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Domain.Entities;

public sealed class Fundraiser : EntityBase
{
    public Guid TenantId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal TargetAmount { get; private set; }
    public decimal CollectedAmount { get; private set; }
    public DateTime? DeadlineUtc { get; private set; }
    public FundraiserStatus Status { get; private set; }
    public int ContributionsCount { get; private set; }
    public bool IsActive { get; private set; }

    private Fundraiser() { } // EF Core

    public static Fundraiser Create(
        Guid tenantId,
        string title,
        string description,
        decimal targetAmount,
        DateTime? deadlineUtc = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        if (targetAmount <= 0)
            throw new ArgumentOutOfRangeException(nameof(targetAmount), "TargetAmount must be greater than zero.");

        var now = DateTime.UtcNow;
        return new Fundraiser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Title = title.Trim(),
            Description = description?.Trim() ?? string.Empty,
            TargetAmount = targetAmount,
            CollectedAmount = 0m,
            DeadlineUtc = deadlineUtc,
            Status = FundraiserStatus.Active,
            ContributionsCount = 0,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void AddContribution(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Contribution amount must be greater than zero.");

        if (Status != FundraiserStatus.Active)
            throw new InvalidOperationException("Cannot contribute to a non-active fundraiser.");

        CollectedAmount += amount;
        ContributionsCount++;
        UpdatedAt = DateTime.UtcNow;

        if (CollectedAmount >= TargetAmount)
        {
            Status = FundraiserStatus.Completed;
        }
    }

    public void Close()
    {
        Status = FundraiserStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = FundraiserStatus.Cancelled;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
