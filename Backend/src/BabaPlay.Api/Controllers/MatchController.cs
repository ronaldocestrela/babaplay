using BabaPlay.Application.Commands.Matches;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Matches;
using BabaPlay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Api.Controllers;

/// <summary>Manages matches within the current tenant.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class MatchController : ControllerBase
{
    private readonly ICommandHandler<CreateMatchCommand, Result<MatchResponse>> _createHandler;
    private readonly IQueryHandler<GetMatchQuery, Result<MatchResponse>> _getHandler;
    private readonly IQueryHandler<GetMatchesQuery, Result<IReadOnlyList<MatchResponse>>> _listHandler;
    private readonly ICommandHandler<UpdateMatchCommand, Result<MatchResponse>> _updateHandler;
    private readonly ICommandHandler<ChangeMatchStatusCommand, Result<MatchResponse>> _changeStatusHandler;
    private readonly ICommandHandler<DeleteMatchCommand, Result> _deleteHandler;

    public MatchController(
        ICommandHandler<CreateMatchCommand, Result<MatchResponse>> createHandler,
        IQueryHandler<GetMatchQuery, Result<MatchResponse>> getHandler,
        IQueryHandler<GetMatchesQuery, Result<IReadOnlyList<MatchResponse>>> listHandler,
        ICommandHandler<UpdateMatchCommand, Result<MatchResponse>> updateHandler,
        ICommandHandler<ChangeMatchStatusCommand, Result<MatchResponse>> changeStatusHandler,
        ICommandHandler<DeleteMatchCommand, Result> deleteHandler)
    {
        _createHandler = createHandler;
        _getHandler = getHandler;
        _listHandler = listHandler;
        _updateHandler = updateHandler;
        _changeStatusHandler = changeStatusHandler;
        _deleteHandler = deleteHandler;
    }

    [HttpPost]
    [ProducesResponseType(typeof(MatchResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateMatchRequest request, CancellationToken ct)
    {
        var result = await _createHandler.HandleAsync(
            new CreateMatchCommand(request.GameDayId, request.HomeTeamId, request.AwayTeamId, request.Description),
            ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode switch
            {
                "MATCH_ALREADY_EXISTS" => StatusCodes.Status409Conflict,
                "GAMEDAY_NOT_FOUND" or "TEAM_NOT_FOUND" => StatusCodes.Status404NotFound,
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

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MatchResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] MatchStatus? status, CancellationToken ct)
    {
        var result = await _listHandler.HandleAsync(new GetMatchesQuery(status), ct);
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _getHandler.HandleAsync(new GetMatchQuery(id), ct);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMatchRequest request, CancellationToken ct)
    {
        var result = await _updateHandler.HandleAsync(
            new UpdateMatchCommand(id, request.GameDayId, request.HomeTeamId, request.AwayTeamId, request.Description),
            ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode switch
            {
                "MATCH_NOT_FOUND" or "GAMEDAY_NOT_FOUND" or "TEAM_NOT_FOUND" => StatusCodes.Status404NotFound,
                "MATCH_ALREADY_EXISTS" => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status422UnprocessableEntity,
            };

            return StatusCode(statusCode, new ProblemDetails
            {
                Status = statusCode,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });
        }

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(MatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeMatchStatusRequest request, CancellationToken ct)
    {
        var result = await _changeStatusHandler.HandleAsync(new ChangeMatchStatusCommand(id, request.Status), ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode == "MATCH_NOT_FOUND"
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
        var result = await _deleteHandler.HandleAsync(new DeleteMatchCommand(id), ct);

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

public sealed record CreateMatchRequest(
    Guid GameDayId,
    Guid HomeTeamId,
    Guid AwayTeamId,
    string? Description);

public sealed record UpdateMatchRequest(
    Guid GameDayId,
    Guid HomeTeamId,
    Guid AwayTeamId,
    string? Description);

public sealed record ChangeMatchStatusRequest(MatchStatus Status);