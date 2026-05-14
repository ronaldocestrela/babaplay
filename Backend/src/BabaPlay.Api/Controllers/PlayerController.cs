using BabaPlay.Application.Commands.Players;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Players;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Api.Controllers;

/// <summary>Manages player profiles within the current tenant.</summary>
/// <remarks>
/// All endpoints require an authenticated user and a valid <c>X-Tenant-Slug</c> header
/// resolved by <see cref="BabaPlay.Api.Middlewares.TenantMiddleware"/>.
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class PlayerController : ControllerBase
{
    private readonly ICommandHandler<CreatePlayerCommand, Result<PlayerResponse>> _createHandler;
    private readonly IQueryHandler<GetPlayerQuery, Result<PlayerResponse>> _getHandler;
    private readonly IQueryHandler<GetPlayersQuery, Result<IReadOnlyList<PlayerResponse>>> _listHandler;
    private readonly ICommandHandler<UpdatePlayerCommand, Result<PlayerResponse>> _updateHandler;
    private readonly ICommandHandler<UpdatePlayerPositionsCommand, Result<PlayerPositionsResponse>> _updatePositionsHandler;
    private readonly ICommandHandler<DeletePlayerCommand, Result> _deleteHandler;

    public PlayerController(
        ICommandHandler<CreatePlayerCommand, Result<PlayerResponse>> createHandler,
        IQueryHandler<GetPlayerQuery, Result<PlayerResponse>> getHandler,
        IQueryHandler<GetPlayersQuery, Result<IReadOnlyList<PlayerResponse>>> listHandler,
        ICommandHandler<UpdatePlayerCommand, Result<PlayerResponse>> updateHandler,
        ICommandHandler<UpdatePlayerPositionsCommand, Result<PlayerPositionsResponse>> updatePositionsHandler,
        ICommandHandler<DeletePlayerCommand, Result> deleteHandler)
    {
        _createHandler = createHandler;
        _getHandler = getHandler;
        _listHandler = listHandler;
        _updateHandler = updateHandler;
        _updatePositionsHandler = updatePositionsHandler;
        _deleteHandler = deleteHandler;
    }

    /// <summary>Creates a new player profile for the given user in this tenant.</summary>
    /// <response code="201">Player created successfully.</response>
    /// <response code="404">Referenced user does not exist (USER_NOT_FOUND).</response>
    /// <response code="409">A player for this user already exists in the tenant (PLAYER_ALREADY_EXISTS).</response>
    /// <response code="422">Validation error, e.g. empty name (INVALID_NAME).</response>
    [HttpPost]
    [ProducesResponseType(typeof(PlayerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreatePlayerRequest request, CancellationToken ct)
    {
        var result = await _createHandler.HandleAsync(
            new CreatePlayerCommand(request.UserId, request.Name, request.Nickname, request.Phone, request.DateOfBirth),
            ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode switch
            {
                "USER_NOT_FOUND" => StatusCodes.Status404NotFound,
                "PLAYER_ALREADY_EXISTS" => StatusCodes.Status409Conflict,
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

    /// <summary>Returns all active players for the current tenant.</summary>
    /// <response code="200">Player list returned (may be empty).</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PlayerResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _listHandler.HandleAsync(new GetPlayersQuery(), ct);
        return Ok(result.Value);
    }

    /// <summary>Returns a single player by id.</summary>
    /// <response code="200">Player found.</response>
    /// <response code="404">Player not found (PLAYER_NOT_FOUND).</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PlayerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _getHandler.HandleAsync(new GetPlayerQuery(id), ct);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });

        return Ok(result.Value);
    }

    /// <summary>Updates an existing player's profile.</summary>
    /// <response code="200">Player updated successfully.</response>
    /// <response code="404">Player not found (PLAYER_NOT_FOUND).</response>
    /// <response code="422">Validation error (INVALID_NAME).</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PlayerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePlayerRequest request, CancellationToken ct)
    {
        var result = await _updateHandler.HandleAsync(
            new UpdatePlayerCommand(id, request.Name, request.Nickname, request.Phone, request.DateOfBirth),
            ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode == "PLAYER_NOT_FOUND"
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

    /// <summary>Replaces the full position list of a player (max 3).</summary>
    /// <response code="200">Player positions updated successfully.</response>
    /// <response code="404">Player or one of the positions was not found.</response>
    /// <response code="422">Position list is invalid (duplicates, empty id, or above limit).</response>
    [HttpPut("{id:guid}/positions")]
    [ProducesResponseType(typeof(PlayerPositionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdatePositions(Guid id, [FromBody] UpdatePlayerPositionsRequest request, CancellationToken ct)
    {
        var result = await _updatePositionsHandler.HandleAsync(
            new UpdatePlayerPositionsCommand(id, request.PositionIds),
            ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode switch
            {
                "PLAYER_NOT_FOUND" => StatusCodes.Status404NotFound,
                "POSITION_NOT_FOUND" => StatusCodes.Status404NotFound,
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

    /// <summary>Soft-deletes a player (sets IsActive = false). Idempotent.</summary>
    /// <response code="204">Player deactivated.</response>
    /// <response code="404">Player not found (PLAYER_NOT_FOUND).</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _deleteHandler.HandleAsync(new DeletePlayerCommand(id), ct);

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

// ---- Request DTOs (local to this file — only used by the API layer) ----

/// <summary>Request body for player creation.</summary>
public sealed record CreatePlayerRequest(
    Guid UserId,
    string Name,
    string? Nickname,
    string? Phone,
    DateOnly? DateOfBirth);

/// <summary>Request body for player update.</summary>
public sealed record UpdatePlayerRequest(
    string Name,
    string? Nickname,
    string? Phone,
    DateOnly? DateOfBirth);

/// <summary>Request body for replacing player position assignments.</summary>
public sealed record UpdatePlayerPositionsRequest(IReadOnlyList<Guid> PositionIds);
