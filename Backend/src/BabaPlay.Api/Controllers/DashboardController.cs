using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = AuthorizationPolicyNames.TenantMember)]
public sealed class DashboardController : ControllerBase
{
    private readonly IQueryHandler<GetDashboardSummaryQuery, Result<DashboardSummaryApplicationDto>> _dashboardQueryHandler;

    public DashboardController(
        IQueryHandler<GetDashboardSummaryQuery, Result<DashboardSummaryApplicationDto>> dashboardQueryHandler)
    {
        _dashboardQueryHandler = dashboardQueryHandler;
    }

    /// <summary>
    /// Returns consolidated summary for the tenant dashboard.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _dashboardQueryHandler.HandleAsync(new GetDashboardSummaryQuery(), cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new ProblemDetails
            {
                Title = result.ErrorCode ?? "DASHBOARD_QUERY_FAILED",
                Detail = result.ErrorMessage
            });
        }

        return Ok(result.Value);
    }
}
