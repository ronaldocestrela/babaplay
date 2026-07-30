namespace BabaPlay.Application.DTOs;

public sealed record DefaultersListResponse(
    DateTime ReferenceUtc,
    int TotalDefaultersCount,
    decimal TotalOverdueAmount,
    double AverageDaysOverdue,
    IReadOnlyList<DefaulterMemberResponse> Items);
