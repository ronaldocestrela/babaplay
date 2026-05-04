using BabaPlay.Application.Commands.Scores;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Scores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = AuthorizationPolicyNames.TenantMember)]
public sealed class RankingController : ControllerBase
{
    private readonly IQueryHandler<GetRankingQuery, Result<IReadOnlyList<RankingEntryResponse>>> _getRankingHandler;
    private readonly IQueryHandler<GetTopScorersQuery, Result<IReadOnlyList<TopScorerEntryResponse>>> _getTopScorersHandler;
    private readonly IQueryHandler<GetAttendanceRankingQuery, Result<IReadOnlyList<AttendanceEntryResponse>>> _getAttendanceHandler;
    private readonly ICommandHandler<RebuildTenantRankingCommand, Result<RebuildRankingResponse>> _rebuildHandler;

    public RankingController(
        IQueryHandler<GetRankingQuery, Result<IReadOnlyList<RankingEntryResponse>>> getRankingHandler,
        IQueryHandler<GetTopScorersQuery, Result<IReadOnlyList<TopScorerEntryResponse>>> getTopScorersHandler,
        IQueryHandler<GetAttendanceRankingQuery, Result<IReadOnlyList<AttendanceEntryResponse>>> getAttendanceHandler,
        ICommandHandler<RebuildTenantRankingCommand, Result<RebuildRankingResponse>> rebuildHandler)
    {
        _getRankingHandler = getRankingHandler;
        _getTopScorersHandler = getTopScorersHandler;
        _getAttendanceHandler = getAttendanceHandler;
        _rebuildHandler = rebuildHandler;
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicyNames.RankingRead)]
    [ProducesResponseType(typeof(IReadOnlyList<RankingEntryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetRanking(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        CancellationToken ct = default)
    {
        var result = await _getRankingHandler.HandleAsync(new GetRankingQuery(page, pageSize, fromUtc, toUtc), ct);

        if (!result.IsSuccess)
            return UnprocessableEntity(new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });

        return Ok(result.Value);
    }

    [HttpGet("top-scorers")]
    [Authorize(Policy = AuthorizationPolicyNames.RankingRead)]
    [ProducesResponseType(typeof(IReadOnlyList<TopScorerEntryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetTopScorers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        CancellationToken ct = default)
    {
        var result = await _getTopScorersHandler.HandleAsync(new GetTopScorersQuery(page, pageSize, fromUtc, toUtc), ct);

        if (!result.IsSuccess)
            return UnprocessableEntity(new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });

        return Ok(result.Value);
    }

    [HttpGet("attendance")]
    [Authorize(Policy = AuthorizationPolicyNames.RankingRead)]
    [ProducesResponseType(typeof(IReadOnlyList<AttendanceEntryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetAttendance(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        CancellationToken ct = default)
    {
        var result = await _getAttendanceHandler.HandleAsync(new GetAttendanceRankingQuery(page, pageSize, fromUtc, toUtc), ct);

        if (!result.IsSuccess)
            return UnprocessableEntity(new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });

        return Ok(result.Value);
    }

    [HttpPost("rebuild")]
    [Authorize(Policy = AuthorizationPolicyNames.RankingWrite)]
    [ProducesResponseType(typeof(RebuildRankingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Rebuild([FromBody] RebuildRankingRequest? request, CancellationToken ct)
    {
        var result = await _rebuildHandler.HandleAsync(
            new RebuildTenantRankingCommand(request?.FromUtc, request?.ToUtc),
            ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode == "INVALID_PERIOD"
                ? StatusCodes.Status422UnprocessableEntity
                : StatusCodes.Status500InternalServerError;

            return StatusCode(statusCode, new ProblemDetails
            {
                Status = statusCode,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });
        }

        return Ok(result.Value);
    }
}

public sealed record RebuildRankingRequest(DateTime? FromUtc, DateTime? ToUtc);