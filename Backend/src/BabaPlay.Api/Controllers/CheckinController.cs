using BabaPlay.Application.Commands.Checkins;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
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

    public CheckinController(ICommandHandler<CreateCheckinCommand, Result<CheckinResponse>> createHandler)
    {
        _createHandler = createHandler;
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
}

public sealed record CreateCheckinRequest(
    Guid PlayerId,
    Guid GameDayId,
    DateTime CheckedInAtUtc,
    double Latitude,
    double Longitude);
