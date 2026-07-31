namespace BabaPlay.Web.Models;

public sealed record DashboardSummaryLoadResult(
    DashboardSummaryDto? Summary,
    string? ErrorMessage);
