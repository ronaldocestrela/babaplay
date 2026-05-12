using BabaPlay.Application.Commands.Tenants;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Tenants;
using BabaPlay.Infrastructure.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BabaPlay.Api.Controllers;

[ApiController]
[Route("api/v1/association-invite")]
public sealed class AssociationInviteController : ControllerBase
{
    private readonly ICommandHandler<SendAssociationInviteCommand, Result<AssociationInviteResponse>> _sendHandler;
    private readonly IQueryHandler<ValidateAssociationInviteQuery, Result<AssociationInviteValidationResponse>> _validateHandler;
    private readonly ICommandHandler<AcceptAssociationInviteCommand, Result<AssociationInviteAcceptResponse>> _acceptHandler;
    private readonly ICommandHandler<RegisterAndAcceptAssociationInviteCommand, Result<AssociationInviteAcceptResponse>> _registerAndAcceptHandler;
    private readonly ITenantContext _tenantContext;
    private readonly AssociationInviteSettings _associationInviteSettings;

    public AssociationInviteController(
        ICommandHandler<SendAssociationInviteCommand, Result<AssociationInviteResponse>> sendHandler,
        IQueryHandler<ValidateAssociationInviteQuery, Result<AssociationInviteValidationResponse>> validateHandler,
        ICommandHandler<AcceptAssociationInviteCommand, Result<AssociationInviteAcceptResponse>> acceptHandler,
        ICommandHandler<RegisterAndAcceptAssociationInviteCommand, Result<AssociationInviteAcceptResponse>> registerAndAcceptHandler,
        ITenantContext tenantContext,
        IOptions<AssociationInviteSettings> associationInviteSettings)
    {
        _sendHandler = sendHandler;
        _validateHandler = validateHandler;
        _acceptHandler = acceptHandler;
        _registerAndAcceptHandler = registerAndAcceptHandler;
        _tenantContext = tenantContext;
        _associationInviteSettings = associationInviteSettings.Value;
    }

    [Authorize(Policy = AuthorizationPolicyNames.TenantMember)]
    [HttpPost]
    [ProducesResponseType(typeof(AssociationInviteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Send([FromBody] SendAssociationInviteRequest request, CancellationToken ct)
    {
        var requestedByUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? string.Empty;

        var result = await _sendHandler.HandleAsync(new SendAssociationInviteCommand(
            _tenantContext.TenantId,
            requestedByUserId,
            request.Email,
            _associationInviteSettings.AcceptLinkBaseUrl,
            _associationInviteSettings.TokenExpiresInHours), ct);

        if (!result.IsSuccess)
            return BuildErrorResult(result.ErrorCode, result.ErrorMessage);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [AllowAnonymous]
    [HttpGet("validate")]
    [ProducesResponseType(typeof(AssociationInviteValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Validate([FromQuery] string token, CancellationToken ct)
    {
        var result = await _validateHandler.HandleAsync(new ValidateAssociationInviteQuery(token), ct);

        if (!result.IsSuccess)
            return BuildErrorResult(result.ErrorCode, result.ErrorMessage);

        return Ok(result.Value);
    }

    [Authorize]
    [HttpPost("accept")]
    [ProducesResponseType(typeof(AssociationInviteAcceptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Accept([FromBody] AcceptAssociationInviteRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? string.Empty;

        var result = await _acceptHandler.HandleAsync(new AcceptAssociationInviteCommand(request.Token, userId), ct);

        if (!result.IsSuccess)
            return BuildErrorResult(result.ErrorCode, result.ErrorMessage);

        return Ok(result.Value);
    }

    [AllowAnonymous]
    [HttpPost("register-accept")]
    [ProducesResponseType(typeof(AssociationInviteAcceptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegisterAndAccept([FromBody] RegisterAndAcceptAssociationInviteRequest request, CancellationToken ct)
    {
        var result = await _registerAndAcceptHandler.HandleAsync(new RegisterAndAcceptAssociationInviteCommand(
            request.Token,
            request.Email,
            request.Password), ct);

        if (!result.IsSuccess)
            return BuildErrorResult(result.ErrorCode, result.ErrorMessage);

        return Ok(result.Value);
    }

    private IActionResult BuildErrorResult(string? errorCode, string? errorMessage)
    {
        var statusCode = errorCode switch
        {
            "FORBIDDEN" => StatusCodes.Status403Forbidden,
            "UNAUTHORIZED" => StatusCodes.Status401Unauthorized,
            "ASSOCIATION_INVITE_INVALID_TOKEN" => StatusCodes.Status401Unauthorized,
            "ASSOCIATION_INVITE_TOKEN_EXPIRED" => StatusCodes.Status401Unauthorized,
            "ASSOCIATION_INVITE_ALREADY_USED" => StatusCodes.Status409Conflict,
            "ASSOCIATION_INVITE_ALREADY_REVOKED" => StatusCodes.Status409Conflict,
            "ASSOCIATION_INVITE_EMAIL_MISMATCH" => StatusCodes.Status409Conflict,
            "ASSOCIATION_INVITE_EMAIL_ALREADY_REGISTERED" => StatusCodes.Status409Conflict,
            "TENANT_NOT_FOUND" => StatusCodes.Status404NotFound,
            "ASSOCIATION_INVITE_EMAIL_REQUIRED" => StatusCodes.Status422UnprocessableEntity,
            "ASSOCIATION_INVITE_EMAIL_INVALID" => StatusCodes.Status422UnprocessableEntity,
            "ASSOCIATION_INVITE_PASSWORD_REQUIRED" => StatusCodes.Status422UnprocessableEntity,
            "ASSOCIATION_INVITE_USER_CREATE_FAILED" => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status422UnprocessableEntity,
        };

        return StatusCode(statusCode, new ProblemDetails
        {
            Status = statusCode,
            Title = errorCode,
            Detail = errorMessage,
        });
    }
}

public sealed record SendAssociationInviteRequest(string Email);

public sealed record AcceptAssociationInviteRequest(string Token);

public sealed record RegisterAndAcceptAssociationInviteRequest(string Token, string Email, string Password);
