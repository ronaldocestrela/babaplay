using BabaPlay.Application.Commands.Tenants;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Tenants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
    private readonly IQueryHandler<GetTenantSettingsQuery, Result<TenantResponse>> _settingsHandler;
    private readonly ICommandHandler<UpdateTenantSettingsCommand, Result<TenantResponse>> _updateSettingsHandler;
    private readonly ITenantContext _tenantContext;

    public TenantController(
        ICommandHandler<CreateTenantCommand, Result<TenantResponse>> createHandler,
        IQueryHandler<GetTenantStatusQuery, Result<TenantResponse>> statusHandler,
        IQueryHandler<GetTenantSettingsQuery, Result<TenantResponse>> settingsHandler,
        ICommandHandler<UpdateTenantSettingsCommand, Result<TenantResponse>> updateSettingsHandler,
        ITenantContext tenantContext)
    {
        _createHandler = createHandler;
        _statusHandler = statusHandler;
        _settingsHandler = settingsHandler;
        _updateSettingsHandler = updateSettingsHandler;
        _tenantContext = tenantContext;
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
    [AllowAnonymous]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(
        [FromForm] CreateTenantRequest request,
        CancellationToken ct)
    {
        TenantLogoUploadRequest? logo = null;
        if (request.Logo is not null)
        {
            await using var ms = new MemoryStream();
            await request.Logo.CopyToAsync(ms, ct);
            logo = new TenantLogoUploadRequest(
                request.Logo.FileName,
                request.Logo.ContentType,
                ms.ToArray());
        }

        var requestedByUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await _createHandler.HandleAsync(
            new CreateTenantCommand(
                request.Name ?? string.Empty,
                request.Slug ?? string.Empty,
                request.AdminEmail,
                request.AdminPassword,
                requestedByUserId,
                logo,
                request.Street ?? string.Empty,
                request.Number ?? string.Empty,
                request.Neighborhood,
                request.City ?? string.Empty,
                request.State ?? string.Empty,
                request.ZipCode ?? string.Empty,
                request.AssociationLatitude,
                request.AssociationLongitude),
            ct);

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
    [AllowAnonymous]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(Guid id, CancellationToken ct)
    {
        var result = await _statusHandler.HandleAsync(new GetTenantStatusQuery(id), ct);
        return Ok(result.Value);
    }

    [Authorize(Policy = AuthorizationPolicyNames.TenantMember)]
    [HttpGet("settings")]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        var result = await _settingsHandler.HandleAsync(new GetTenantSettingsQuery(_tenantContext.TenantId), ct);
        return Ok(result.Value);
    }

    [Authorize(Policy = AuthorizationPolicyNames.TenantMember)]
    [HttpPut("settings")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateSettings([FromForm] UpdateTenantSettingsRequest request, CancellationToken ct)
    {
        TenantLogoUploadRequest? logo = null;
        if (request.Logo is not null)
        {
            await using var ms = new MemoryStream();
            await request.Logo.CopyToAsync(ms, ct);
            logo = new TenantLogoUploadRequest(
                request.Logo.FileName,
                request.Logo.ContentType,
                ms.ToArray());
        }

        var requestedByUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? string.Empty;

        var result = await _updateSettingsHandler.HandleAsync(
            new UpdateTenantSettingsCommand(
                _tenantContext.TenantId,
                requestedByUserId,
                request.Name ?? string.Empty,
                request.PlayersPerTeam,
                logo,
                request.Street ?? string.Empty,
                request.Number ?? string.Empty,
                request.Neighborhood,
                request.City ?? string.Empty,
                request.State ?? string.Empty,
                request.ZipCode ?? string.Empty,
                request.AssociationLatitude,
                request.AssociationLongitude),
            ct);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode == "FORBIDDEN"
                ? StatusCodes.Status403Forbidden
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
}

/// <summary>Request body for tenant creation.</summary>
public sealed record CreateTenantRequest(
    string? Name,
    string? Slug,
    IFormFile? Logo,
    string? Street,
    string? Number,
    string? Neighborhood,
    string? City,
    string? State,
    string? ZipCode,
    double AssociationLatitude,
    double AssociationLongitude,
    string? AdminEmail = null,
    string? AdminPassword = null);

public sealed record UpdateTenantSettingsRequest(
    string? Name,
    int PlayersPerTeam,
    IFormFile? Logo,
    string? Street,
    string? Number,
    string? Neighborhood,
    string? City,
    string? State,
    string? ZipCode,
    double AssociationLatitude,
    double AssociationLongitude);
