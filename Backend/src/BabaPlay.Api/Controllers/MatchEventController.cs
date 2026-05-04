using BabaPlay.Application.Commands.MatchEvents;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.MatchEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Api.Controllers;

/// <summary>Manages match timeline events within the current tenant.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class MatchEventController : ControllerBase
{
    private readonly ICommandHandler<CreateMatchEventCommand, Result<MatchEventResponse>> _createHandler;
    private readonly ICommandHandler<UpdateMatchEventCommand, Result<MatchEventResponse>> _updateHandler;
    private readonly ICommandHandler<DeleteMatchEventCommand, Result> _deleteHandler;
    private readonly IQueryHandler<GetMatchEventQuery, Result<MatchEventResponse>> _getHandler;
    private readonly IQueryHandler<GetMatchEventsByMatchQuery, Result<IReadOnlyList<MatchEventResponse>>> _getByMatchHandler;
    private readonly IQueryHandler<GetMatchEventsByPlayerQuery, Result<IReadOnlyList<MatchEventResponse>>> _getByPlayerHandler;

    public MatchEventController(
        ICommandHandler<CreateMatchEventCommand, Result<MatchEventResponse>> createHandler,
        ICommandHandler<UpdateMatchEventCommand, Result<MatchEventResponse>> updateHandler,
        ICommandHandler<DeleteMatchEventCommand, Result> deleteHandler,
        IQueryHandler<GetMatchEventQuery, Result<MatchEventResponse>> getHandler,
        IQueryHandler<GetMatchEventsByMatchQuery, Result<IReadOnlyList<MatchEventResponse>>> getByMatchHandler,
        IQueryHandler<GetMatchEventsByPlayerQuery, Result<IReadOnlyList<MatchEventResponse>>> getByPlayerHandler)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _getHandler = getHandler;
        _getByMatchHandler = getByMatchHandler;
        _getByPlayerHandler = getByPlayerHandler;
    }

    [HttpPost]
    [ProducesResponseType(typeof(MatchEventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateMatchEventRequest request, CancellationToken ct)
    {
        var result = await _createHandler.HandleAsync(
            new CreateMatchEventCommand(
                request.MatchId,
                request.TeamId,
                request.PlayerId,
                request.MatchEventTypeId,
                request.Minute,
                request.Notes),
            ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode switch
            {
                "MATCH_EVENT_MATCH_NOT_FOUND" or "MATCH_EVENT_TEAM_NOT_FOUND" or "MATCH_EVENT_PLAYER_NOT_FOUND" or "MATCH_EVENT_TYPE_NOT_FOUND"
                    => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status422UnprocessableEntity,
            };

            return StatusCode(statusCode, new ProblemDetails
            {
                Status = statusCode,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MatchEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _getHandler.HandleAsync(new GetMatchEventQuery(id), ct);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });

        return Ok(result.Value);
    }

    [HttpGet("match/{matchId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<MatchEventResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByMatch(Guid matchId, CancellationToken ct)
    {
        var result = await _getByMatchHandler.HandleAsync(new GetMatchEventsByMatchQuery(matchId), ct);
        return Ok(result.Value);
    }

    [HttpGet("player/{playerId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<MatchEventResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPlayer(Guid playerId, CancellationToken ct)
    {
        var result = await _getByPlayerHandler.HandleAsync(new GetMatchEventsByPlayerQuery(playerId), ct);
        return Ok(result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MatchEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMatchEventRequest request, CancellationToken ct)
    {
        var result = await _updateHandler.HandleAsync(
            new UpdateMatchEventCommand(id, request.MatchEventTypeId, request.Minute, request.Notes),
            ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode is "MATCH_EVENT_NOT_FOUND" or "MATCH_EVENT_TYPE_NOT_FOUND"
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status422UnprocessableEntity;

            return StatusCode(statusCode, new ProblemDetails
            {
                Status = statusCode,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _deleteHandler.HandleAsync(new DeleteMatchEventCommand(id), ct);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });

        return NoContent();
    }
}

public sealed record CreateMatchEventRequest(
    Guid MatchId,
    Guid TeamId,
    Guid PlayerId,
    Guid MatchEventTypeId,
    int Minute,
    string? Notes);

public sealed record UpdateMatchEventRequest(
    Guid MatchEventTypeId,
    int Minute,
    string? Notes);
