using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Dashboard;

/// <summary>Query to retrieve consolidated dashboard summary for the active tenant.</summary>
public sealed record GetDashboardSummaryQuery(string? RequestingUserId = null)
    : IQuery<Result<DashboardSummaryApplicationDto>>;
