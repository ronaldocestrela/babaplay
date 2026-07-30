using System;
using BabaPlay.Domain.Enums;

namespace BabaPlay.Application.DTOs;

public sealed record FundraiserResponse(
    Guid Id,
    Guid TenantId,
    string Title,
    string Description,
    decimal TargetAmount,
    decimal CollectedAmount,
    double ProgressPercentage,
    DateTime? DeadlineUtc,
    FundraiserStatus Status,
    int ContributionsCount,
    DateTime CreatedAt);
