namespace BabaPlay.Application.DTOs;

public sealed record DefaulterMemberResponse(
    Guid PlayerId,
    string PlayerName,
    int OverdueInvoicesCount,
    decimal TotalOverdueAmount,
    int MaxDaysOverdue,
    IReadOnlyList<string> OverdueCompetences,
    DateTime? LastReminderSentAtUtc);
