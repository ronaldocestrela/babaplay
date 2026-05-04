using BabaPlay.Application.Commands.MatchEvents;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.MatchEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Api.Controllers;

/// <summary>Manages tenant-configurable match event types.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = AuthorizationPolicyNames.TenantMember)]
public sealed class MatchEventTypeController : ControllerBase
{
    private readonly ICommandHandler<CreateMatchEventTypeCommand, Result<MatchEventTypeResponse>> _createHandler;
    private readonly ICommandHandler<UpdateMatchEventTypeCommand, Result<MatchEventTypeResponse>> _updateHandler;
    private readonly ICommandHandler<DeleteMatchEventTypeCommand, Result> _deleteHandler;
    private readonly IQueryHandler<GetMatchEventTypeQuery, Result<MatchEventTypeResponse>> _getHandler;
    private readonly IQueryHandler<GetMatchEventTypesQuery, Result<IReadOnlyList<MatchEventTypeResponse>>> _listHandler;

    public MatchEventTypeController(
        ICommandHandler<CreateMatchEventTypeCommand, Result<MatchEventTypeResponse>> createHandler,
        ICommandHandler<UpdateMatchEventTypeCommand, Result<MatchEventTypeResponse>> updateHandler,
        ICommandHandler<DeleteMatchEventTypeCommand, Result> deleteHandler,
        IQueryHandler<GetMatchEventTypeQuery, Result<MatchEventTypeResponse>> getHandler,
        IQueryHandler<GetMatchEventTypesQuery, Result<IReadOnlyList<MatchEventTypeResponse>>> listHandler)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _getHandler = getHandler;
        _listHandler = listHandler;
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicyNames.MatchEventTypesWrite)]
    [ProducesResponseType(typeof(MatchEventTypeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateMatchEventTypeRequest request, CancellationToken ct)
    {
        var result = await _createHandler.HandleAsync(
            new CreateMatchEventTypeCommand(request.Code, request.Name, request.Points, request.IsSystemDefault),
            ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode == "MATCH_EVENT_TYPE_ALREADY_EXISTS"
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
    [Authorize(Policy = AuthorizationPolicyNames.MatchEventTypesRead)]
    [ProducesResponseType(typeof(IReadOnlyList<MatchEventTypeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _listHandler.HandleAsync(new GetMatchEventTypesQuery(), ct);
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.MatchEventTypesRead)]
    [ProducesResponseType(typeof(MatchEventTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _getHandler.HandleAsync(new GetMatchEventTypeQuery(id), ct);

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
    [Authorize(Policy = AuthorizationPolicyNames.MatchEventTypesWrite)]
    [ProducesResponseType(typeof(MatchEventTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMatchEventTypeRequest request, CancellationToken ct)
    {
        var result = await _updateHandler.HandleAsync(
            new UpdateMatchEventTypeCommand(id, request.Code, request.Name, request.Points),
            ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode switch
            {
                "MATCH_EVENT_TYPE_NOT_FOUND" => StatusCodes.Status404NotFound,
                "MATCH_EVENT_TYPE_ALREADY_EXISTS" => StatusCodes.Status409Conflict,
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

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.MatchEventTypesWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _deleteHandler.HandleAsync(new DeleteMatchEventTypeCommand(id), ct);

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

public sealed record CreateMatchEventTypeRequest(
    string Code,
    string Name,
    int Points,
    bool IsSystemDefault);

public sealed record UpdateMatchEventTypeRequest(
    string Code,
    string Name,
    int Points);
