using BabaPlay.Application.Commands.GameDays;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.GameDays;
using BabaPlay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Api.Controllers;

/// <summary>Manages game days within the current tenant.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class GameDayController : ControllerBase
{
    private readonly ICommandHandler<CreateGameDayCommand, Result<GameDayResponse>> _createHandler;
    private readonly IQueryHandler<GetGameDayQuery, Result<GameDayResponse>> _getHandler;
    private readonly IQueryHandler<GetGameDaysQuery, Result<IReadOnlyList<GameDayResponse>>> _listHandler;
    private readonly ICommandHandler<UpdateGameDayCommand, Result<GameDayResponse>> _updateHandler;
    private readonly ICommandHandler<ChangeGameDayStatusCommand, Result<GameDayResponse>> _changeStatusHandler;
    private readonly ICommandHandler<DeleteGameDayCommand, Result> _deleteHandler;

    public GameDayController(
        ICommandHandler<CreateGameDayCommand, Result<GameDayResponse>> createHandler,
        IQueryHandler<GetGameDayQuery, Result<GameDayResponse>> getHandler,
        IQueryHandler<GetGameDaysQuery, Result<IReadOnlyList<GameDayResponse>>> listHandler,
        ICommandHandler<UpdateGameDayCommand, Result<GameDayResponse>> updateHandler,
        ICommandHandler<ChangeGameDayStatusCommand, Result<GameDayResponse>> changeStatusHandler,
        ICommandHandler<DeleteGameDayCommand, Result> deleteHandler)
    {
        _createHandler = createHandler;
        _getHandler = getHandler;
        _listHandler = listHandler;
        _updateHandler = updateHandler;
        _changeStatusHandler = changeStatusHandler;
        _deleteHandler = deleteHandler;
    }

    [HttpPost]
    [ProducesResponseType(typeof(GameDayResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateGameDayRequest request, CancellationToken ct)
    {
        var result = await _createHandler.HandleAsync(
            new CreateGameDayCommand(request.Name, request.ScheduledAt, request.Location, request.Description, request.MaxPlayers),
            ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode == "GAMEDAY_ALREADY_EXISTS"
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status422UnprocessableEntity;

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
    [ProducesResponseType(typeof(IReadOnlyList<GameDayResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GameDayStatus? status, CancellationToken ct)
    {
        var result = await _listHandler.HandleAsync(new GetGameDaysQuery(status), ct);
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GameDayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _getHandler.HandleAsync(new GetGameDayQuery(id), ct);

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
    [ProducesResponseType(typeof(GameDayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGameDayRequest request, CancellationToken ct)
    {
        var result = await _updateHandler.HandleAsync(
            new UpdateGameDayCommand(id, request.Name, request.ScheduledAt, request.Location, request.Description, request.MaxPlayers),
            ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode switch
            {
                "GAMEDAY_NOT_FOUND" => StatusCodes.Status404NotFound,
                "GAMEDAY_ALREADY_EXISTS" => StatusCodes.Status409Conflict,
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
    [ProducesResponseType(typeof(GameDayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeGameDayStatusRequest request, CancellationToken ct)
    {
        var result = await _changeStatusHandler.HandleAsync(
            new ChangeGameDayStatusCommand(id, request.Status),
            ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode == "GAMEDAY_NOT_FOUND"
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
        var result = await _deleteHandler.HandleAsync(new DeleteGameDayCommand(id), ct);

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

public sealed record CreateGameDayRequest(
    string Name,
    DateTime ScheduledAt,
    string? Location,
    string? Description,
    int MaxPlayers);

public sealed record UpdateGameDayRequest(
    string Name,
    DateTime ScheduledAt,
    string? Location,
    string? Description,
    int MaxPlayers);

public sealed record ChangeGameDayStatusRequest(GameDayStatus Status);
