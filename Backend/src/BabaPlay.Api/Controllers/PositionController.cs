using BabaPlay.Application.Commands.Positions;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Positions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Api.Controllers;

/// <summary>Manages tenant positions.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = AuthorizationPolicyNames.TenantMember)]
public sealed class PositionController : ControllerBase
{
    private readonly ICommandHandler<CreatePositionCommand, Result<PositionResponse>> _createHandler;
    private readonly IQueryHandler<GetPositionQuery, Result<PositionResponse>> _getHandler;
    private readonly IQueryHandler<GetPositionsQuery, Result<IReadOnlyList<PositionResponse>>> _listHandler;
    private readonly ICommandHandler<UpdatePositionCommand, Result<PositionResponse>> _updateHandler;
    private readonly ICommandHandler<DeletePositionCommand, Result> _deleteHandler;

    public PositionController(
        ICommandHandler<CreatePositionCommand, Result<PositionResponse>> createHandler,
        IQueryHandler<GetPositionQuery, Result<PositionResponse>> getHandler,
        IQueryHandler<GetPositionsQuery, Result<IReadOnlyList<PositionResponse>>> listHandler,
        ICommandHandler<UpdatePositionCommand, Result<PositionResponse>> updateHandler,
        ICommandHandler<DeletePositionCommand, Result> deleteHandler)
    {
        _createHandler = createHandler;
        _getHandler = getHandler;
        _listHandler = listHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
    }

    /// <summary>Creates a position in the current tenant.</summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicyNames.TenantOwner)]
    [ProducesResponseType(typeof(PositionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreatePositionRequest request, CancellationToken ct)
    {
        var result = await _createHandler.HandleAsync(
            new CreatePositionCommand(request.Code, request.Name, request.Description),
            ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode == "POSITION_ALREADY_EXISTS"
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

    /// <summary>Returns all active positions from current tenant.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PositionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _listHandler.HandleAsync(new GetPositionsQuery(), ct);
        return Ok(result.Value);
    }

    /// <summary>Returns one position by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PositionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _getHandler.HandleAsync(new GetPositionQuery(id), ct);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });

        return Ok(result.Value);
    }

    /// <summary>Updates a position in the current tenant.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.TenantOwner)]
    [ProducesResponseType(typeof(PositionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePositionRequest request, CancellationToken ct)
    {
        var result = await _updateHandler.HandleAsync(
            new UpdatePositionCommand(id, request.Code, request.Name, request.Description),
            ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode switch
            {
                "POSITION_NOT_FOUND" => StatusCodes.Status404NotFound,
                "POSITION_ALREADY_EXISTS" => StatusCodes.Status409Conflict,
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

    /// <summary>Soft deletes a position in the current tenant.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.TenantOwner)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _deleteHandler.HandleAsync(new DeletePositionCommand(id), ct);

        if (!result.IsSuccess)
            return StatusCode(
                result.ErrorCode == "POSITION_IN_USE"
                    ? StatusCodes.Status409Conflict
                    : StatusCodes.Status404NotFound,
                new ProblemDetails
            {
                Status = result.ErrorCode == "POSITION_IN_USE"
                    ? StatusCodes.Status409Conflict
                    : StatusCodes.Status404NotFound,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });

        return NoContent();
    }
}

public sealed record CreatePositionRequest(string Code, string Name, string? Description);

public sealed record UpdatePositionRequest(string Code, string Name, string? Description);
