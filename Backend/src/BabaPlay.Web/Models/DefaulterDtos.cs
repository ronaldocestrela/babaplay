using System;
using System.Collections.Generic;

namespace BabaPlay.Web.Models;

public sealed record DefaulterMemberDto
{
    public Guid PlayerId { get; init; }
    public string PlayerName { get; init; } = string.Empty;
    public int OverdueInvoicesCount { get; init; }
    public decimal TotalOverdueAmount { get; init; }
    public int MaxDaysOverdue { get; init; }
    public List<string> OverdueCompetences { get; init; } = new();
    public DateTime? LastReminderSentAtUtc { get; init; }
}

public sealed record DefaultersListDto
{
    public DateTime ReferenceUtc { get; init; }
    public int TotalDefaultersCount { get; init; }
    public decimal TotalOverdueAmount { get; init; }
    public double AverageDaysOverdue { get; init; }
    public List<DefaulterMemberDto> Items { get; init; } = new();
}
