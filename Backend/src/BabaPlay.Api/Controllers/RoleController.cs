using BabaPlay.Application.Commands.Roles;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Api.Controllers;

/// <summary>Manages tenant-scoped roles and permissions.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "TenantMember")]
public sealed class RoleController : ControllerBase
{
    private readonly ICommandHandler<CreateRoleCommand, Result<RoleResponse>> _createRoleHandler;
    private readonly IQueryHandler<GetRolesQuery, Result<IReadOnlyList<RoleResponse>>> _listRolesHandler;
    private readonly ICommandHandler<AssignRoleToUserCommand, Result> _assignRoleHandler;
    private readonly ICommandHandler<AddPermissionToRoleCommand, Result<RoleResponse>> _addPermissionHandler;

    public RoleController(
        ICommandHandler<CreateRoleCommand, Result<RoleResponse>> createRoleHandler,
        IQueryHandler<GetRolesQuery, Result<IReadOnlyList<RoleResponse>>> listRolesHandler,
        ICommandHandler<AssignRoleToUserCommand, Result> assignRoleHandler,
        ICommandHandler<AddPermissionToRoleCommand, Result<RoleResponse>> addPermissionHandler)
    {
        _createRoleHandler = createRoleHandler;
        _listRolesHandler = listRolesHandler;
        _assignRoleHandler = assignRoleHandler;
        _addPermissionHandler = addPermissionHandler;
    }

    [HttpPost]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        var result = await _createRoleHandler.HandleAsync(new CreateRoleCommand(request.Name, request.Description), ct);

        if (!result.IsSuccess)
            return Error(result.ErrorCode, result.ErrorMessage);

        return Created($"/api/v1/role/{result.Value!.Id}", result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _listRolesHandler.HandleAsync(new GetRolesQuery(), ct);
        return Ok(result.Value);
    }

    [HttpPost("{roleId:guid}/users/{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AssignUser(Guid roleId, string userId, CancellationToken ct)
    {
        var result = await _assignRoleHandler.HandleAsync(new AssignRoleToUserCommand(userId, roleId), ct);

        if (!result.IsSuccess)
            return Error(result.ErrorCode, result.ErrorMessage);

        return NoContent();
    }

    [HttpPost("{roleId:guid}/permissions")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddPermission(
        Guid roleId,
        [FromBody] AddPermissionRequest request,
        CancellationToken ct)
    {
        var result = await _addPermissionHandler.HandleAsync(
            new AddPermissionToRoleCommand(roleId, request.Code, request.Description, request.IsSystemPermission),
            ct);

        if (!result.IsSuccess)
            return Error(result.ErrorCode, result.ErrorMessage);

        return Ok(result.Value);
    }

    private IActionResult Error(string? code, string? detail)
    {
        var statusCode = code switch
        {
            "ROLE_ALREADY_EXISTS" => StatusCodes.Status409Conflict,
            "ROLE_ALREADY_ASSIGNED" => StatusCodes.Status409Conflict,
            "ROLE_NOT_FOUND" => StatusCodes.Status404NotFound,
            "USER_NOT_FOUND" => StatusCodes.Status404NotFound,
            "USER_NOT_IN_TENANT" => StatusCodes.Status422UnprocessableEntity,
            "TENANT_NOT_RESOLVED" => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status422UnprocessableEntity,
        };

        return StatusCode(statusCode, new ProblemDetails
        {
            Status = statusCode,
            Title = code,
            Detail = detail,
        });
    }
}

public sealed record CreateRoleRequest(string Name, string? Description);
public sealed record AddPermissionRequest(string Code, string? Description, bool IsSystemPermission = true);
