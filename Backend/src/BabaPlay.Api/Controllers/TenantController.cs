using BabaPlay.Application.Commands.Tenants;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Tenants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Api.Controllers;

/// <summary>Manages tenant lifecycle: creation and provisioning status.</summary>
[ApiController]
[Route("api/v1/[controller]")]
public sealed class TenantController : ControllerBase
{
    private readonly ICommandHandler<CreateTenantCommand, Result<TenantResponse>> _createHandler;
    private readonly IQueryHandler<GetTenantStatusQuery, Result<TenantResponse>> _statusHandler;

    public TenantController(
        ICommandHandler<CreateTenantCommand, Result<TenantResponse>> createHandler,
        IQueryHandler<GetTenantStatusQuery, Result<TenantResponse>> statusHandler)
    {
        _createHandler = createHandler;
        _statusHandler = statusHandler;
    }

    /// <summary>
    /// Creates a new tenant and enqueues async database provisioning.
    /// </summary>
    /// <remarks>
    /// The tenant is created immediately with status <c>Pending</c>.
    /// Poll <c>GET /api/v1/tenant/{id}/status</c> until status is <c>Ready</c>.
    /// </remarks>
    /// <response code="201">Tenant created; provisioning enqueued.</response>
    /// <response code="409">Slug is already taken (TENANT_SLUG_TAKEN).</response>
    /// <response code="422">Validation error (name or slug empty).</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTenantRequest request,
        CancellationToken ct)
    {
        var result = await _createHandler.HandleAsync(
            new CreateTenantCommand(request.Name, request.Slug), ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode == "TENANT_SLUG_TAKEN"
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status422UnprocessableEntity;

            return StatusCode(statusCode, new ProblemDetails
            {
                Status = statusCode,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });
        }

        return CreatedAtAction(
            nameof(GetStatus),
            new { id = result.Value!.Id },
            result.Value);
    }

    /// <summary>Returns the current provisioning status for a tenant.</summary>
    /// <response code="200">Tenant found; status returned.</response>
    /// <response code="404">Tenant not found (TENANT_NOT_FOUND).</response>
    [HttpGet("{id:guid}/status")]
    [Authorize]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(Guid id, CancellationToken ct)
    {
        var result = await _statusHandler.HandleAsync(new GetTenantStatusQuery(id), ct);
        return Ok(result.Value);
    }
}

/// <summary>Request body for tenant creation.</summary>
public sealed record CreateTenantRequest(string Name, string Slug);
