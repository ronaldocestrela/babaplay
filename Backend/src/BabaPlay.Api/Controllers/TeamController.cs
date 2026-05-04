using BabaPlay.Application.Commands.Teams;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Teams;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Api.Controllers;

/// <summary>Manages teams within the current tenant.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class TeamController : ControllerBase
{
    private readonly ICommandHandler<CreateTeamCommand, Result<TeamResponse>> _createHandler;
    private readonly IQueryHandler<GetTeamQuery, Result<TeamResponse>> _getHandler;
    private readonly IQueryHandler<GetTeamsQuery, Result<IReadOnlyList<TeamResponse>>> _listHandler;
    private readonly ICommandHandler<UpdateTeamCommand, Result<TeamResponse>> _updateHandler;
    private readonly ICommandHandler<UpdateTeamPlayersCommand, Result<TeamPlayersResponse>> _updatePlayersHandler;
    private readonly ICommandHandler<DeleteTeamCommand, Result> _deleteHandler;

    public TeamController(
        ICommandHandler<CreateTeamCommand, Result<TeamResponse>> createHandler,
        IQueryHandler<GetTeamQuery, Result<TeamResponse>> getHandler,
        IQueryHandler<GetTeamsQuery, Result<IReadOnlyList<TeamResponse>>> listHandler,
        ICommandHandler<UpdateTeamCommand, Result<TeamResponse>> updateHandler,
        ICommandHandler<UpdateTeamPlayersCommand, Result<TeamPlayersResponse>> updatePlayersHandler,
        ICommandHandler<DeleteTeamCommand, Result> deleteHandler)
    {
        _createHandler = createHandler;
        _getHandler = getHandler;
        _listHandler = listHandler;
        _updateHandler = updateHandler;
        _updatePlayersHandler = updatePlayersHandler;
        _deleteHandler = deleteHandler;
    }

    /// <summary>Creates a team in the current tenant.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TeamResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateTeamRequest request, CancellationToken ct)
    {
        var result = await _createHandler.HandleAsync(new CreateTeamCommand(request.Name, request.MaxPlayers), ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode == "TEAM_ALREADY_EXISTS"
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

    /// <summary>Returns all active teams from current tenant.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TeamResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _listHandler.HandleAsync(new GetTeamsQuery(), ct);
        return Ok(result.Value);
    }

    /// <summary>Returns one team by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TeamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _getHandler.HandleAsync(new GetTeamQuery(id), ct);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });

        return Ok(result.Value);
    }

    /// <summary>Updates a team in the current tenant.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TeamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeamRequest request, CancellationToken ct)
    {
        var result = await _updateHandler.HandleAsync(new UpdateTeamCommand(id, request.Name, request.MaxPlayers), ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode switch
            {
                "TEAM_NOT_FOUND" => StatusCodes.Status404NotFound,
                "TEAM_ALREADY_EXISTS" => StatusCodes.Status409Conflict,
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

    /// <summary>Replaces the full player list of a team.</summary>
    [HttpPut("{id:guid}/players")]
    [ProducesResponseType(typeof(TeamPlayersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdatePlayers(Guid id, [FromBody] UpdateTeamPlayersRequest request, CancellationToken ct)
    {
        var result = await _updatePlayersHandler.HandleAsync(new UpdateTeamPlayersCommand(id, request.PlayerIds), ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode switch
            {
                "TEAM_NOT_FOUND" => StatusCodes.Status404NotFound,
                "TEAM_PLAYER_NOT_FOUND" => StatusCodes.Status404NotFound,
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

    /// <summary>Soft deletes a team in the current tenant.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _deleteHandler.HandleAsync(new DeleteTeamCommand(id), ct);

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

public sealed record CreateTeamRequest(string Name, int MaxPlayers);

public sealed record UpdateTeamRequest(string Name, int MaxPlayers);

public sealed record UpdateTeamPlayersRequest(IReadOnlyList<Guid> PlayerIds);