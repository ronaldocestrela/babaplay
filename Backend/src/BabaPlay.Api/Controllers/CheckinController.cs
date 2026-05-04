using BabaPlay.Application.Commands.Checkins;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Checkins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Api.Controllers;

/// <summary>Manages player check-ins within the current tenant.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class CheckinController : ControllerBase
{
    private readonly ICommandHandler<CreateCheckinCommand, Result<CheckinResponse>> _createHandler;
    private readonly ICommandHandler<CancelCheckinCommand, Result> _cancelHandler;
    private readonly IQueryHandler<GetCheckinsByGameDayQuery, Result<IReadOnlyList<CheckinResponse>>> _getByGameDayHandler;
    private readonly IQueryHandler<GetCheckinsByPlayerQuery, Result<IReadOnlyList<CheckinResponse>>> _getByPlayerHandler;

    public CheckinController(
        ICommandHandler<CreateCheckinCommand, Result<CheckinResponse>> createHandler,
        ICommandHandler<CancelCheckinCommand, Result> cancelHandler,
        IQueryHandler<GetCheckinsByGameDayQuery, Result<IReadOnlyList<CheckinResponse>>> getByGameDayHandler,
        IQueryHandler<GetCheckinsByPlayerQuery, Result<IReadOnlyList<CheckinResponse>>> getByPlayerHandler)
    {
        _createHandler = createHandler;
        _cancelHandler = cancelHandler;
        _getByGameDayHandler = getByGameDayHandler;
        _getByPlayerHandler = getByPlayerHandler;
    }

    /// <summary>Creates a new check-in for a player in a game day.</summary>
    /// <response code="201">Check-in created successfully.</response>
    /// <response code="404">Player or game day not found.</response>
    /// <response code="409">Player already checked in for this game day.</response>
    /// <response code="422">Validation/business rule violation.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CheckinResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateCheckinRequest request, CancellationToken ct)
    {
        var result = await _createHandler.HandleAsync(new CreateCheckinCommand(
            request.PlayerId,
            request.GameDayId,
            request.CheckedInAtUtc,
            request.Latitude,
            request.Longitude), ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode switch
            {
                "PLAYER_NOT_FOUND" => StatusCodes.Status404NotFound,
                "GAMEDAY_NOT_FOUND" => StatusCodes.Status404NotFound,
                "CHECKIN_ALREADY_EXISTS" => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status422UnprocessableEntity,
            };

            return StatusCode(statusCode, new ProblemDetails
            {
                Status = statusCode,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });
        }

        return CreatedAtAction(nameof(Create), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Returns active check-ins for a game day.</summary>
    [HttpGet("gameday/{gameDayId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<CheckinResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByGameDay(Guid gameDayId, CancellationToken ct)
    {
        var result = await _getByGameDayHandler.HandleAsync(new GetCheckinsByGameDayQuery(gameDayId), ct);
        return Ok(result.Value);
    }

    /// <summary>Returns active check-ins for a player.</summary>
    [HttpGet("player/{playerId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<CheckinResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPlayer(Guid playerId, CancellationToken ct)
    {
        var result = await _getByPlayerHandler.HandleAsync(new GetCheckinsByPlayerQuery(playerId), ct);
        return Ok(result.Value);
    }

    /// <summary>Cancels a check-in (idempotent when already inactive).</summary>
    /// <response code="204">Check-in cancelled successfully or already inactive.</response>
    /// <response code="404">Check-in not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var result = await _cancelHandler.HandleAsync(new CancelCheckinCommand(id), ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode == "CHECKIN_NOT_FOUND"
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status422UnprocessableEntity;

            return StatusCode(statusCode, new ProblemDetails
            {
                Status = statusCode,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });
        }

        return NoContent();
    }
}

public sealed record CreateCheckinRequest(
    Guid PlayerId,
    Guid GameDayId,
    DateTime CheckedInAtUtc,
    double Latitude,
    double Longitude);
