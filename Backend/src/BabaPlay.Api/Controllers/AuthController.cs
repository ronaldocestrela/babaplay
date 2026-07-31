using BabaPlay.Application.Commands.Auth;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly ICommandHandler<LoginCommand, Result<AuthResponse>> _loginHandler;
    private readonly ICommandHandler<RefreshTokenCommand, Result<AuthResponse>> _refreshTokenHandler;
    private readonly IUserRepository _userRepository;
    private readonly IUserTenantRepository _userTenantRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly ICommandHandler<ForgotPasswordCommand, Result> _forgotPasswordHandler;
    private readonly ICommandHandler<ResetPasswordCommand, Result> _resetPasswordHandler;

    public AuthController(
        ICommandHandler<LoginCommand, Result<AuthResponse>> loginHandler,
        ICommandHandler<RefreshTokenCommand, Result<AuthResponse>> refreshTokenHandler,
        IUserRepository userRepository,
        IUserTenantRepository userTenantRepository,
        IUserRoleRepository userRoleRepository,
        ICommandHandler<ForgotPasswordCommand, Result> forgotPasswordHandler,
        ICommandHandler<ResetPasswordCommand, Result> resetPasswordHandler)
    {
        _loginHandler = loginHandler;
        _refreshTokenHandler = refreshTokenHandler;
        _userRepository = userRepository;
        _userTenantRepository = userTenantRepository;
        _userRoleRepository = userRoleRepository;
        _forgotPasswordHandler = forgotPasswordHandler;
        _resetPasswordHandler = resetPasswordHandler;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT access token plus a rotating refresh token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _loginHandler.HandleAsync(new LoginCommand(request.Email, request.Password), cancellationToken);

        if (!result.IsSuccess)
        {
            var statusCode = result.ErrorCode == "USER_INACTIVE"
                ? StatusCodes.Status422UnprocessableEntity
                : StatusCodes.Status401Unauthorized;

            return StatusCode(statusCode, new ProblemDetails
            {
                Status = statusCode,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Exchanges a valid refresh token for a new access/refresh token pair (rotation — old token is revoked).
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _refreshTokenHandler.HandleAsync(new RefreshTokenCommand(request.RefreshToken), cancellationToken);

        if (!result.IsSuccess)
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage,
            });

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns the current authenticated user profile and tenant memberships.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name);

        var email = User.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? User.FindFirstValue(ClaimTypes.Email);

        UserAuthDto? user = null;
        if (!string.IsNullOrWhiteSpace(userId))
            user = await _userRepository.FindByIdAsync(userId, cancellationToken);

        if (user is null && !string.IsNullOrWhiteSpace(email))
            user = await _userRepository.FindByEmailAsync(email, cancellationToken);

        if (user is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "UNAUTHORIZED",
                Detail = "Authenticated user could not be resolved from token claims.",
            });
        }

        var roles = await _userRepository.GetRolesAsync(user.Id, cancellationToken);
        var memberships = await _userTenantRepository.GetMembershipsAsync(user.Id, cancellationToken);

        return Ok(new UserProfileResponse(
            user.Id,
            user.Email,
            roles,
            user.IsActive,
            user.CreatedAt,
            memberships.FirstOrDefault(),
            memberships));
    }

    /// <summary>
    /// Returns RBAC permission codes for the authenticated user in the current tenant context.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicyNames.TenantMember)]
    [HttpGet("me/permissions")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MyPermissions(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "UNAUTHORIZED",
                Detail = "Authenticated user could not be resolved from token claims.",
            });
        }

        var permissions = await _userRoleRepository.GetPermissionCodesAsync(userId, cancellationToken);
        return Ok(permissions);
    }

    /// <summary>
    /// Generates a password reset token and queues a recovery email.
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _forgotPasswordHandler.HandleAsync(
            new ForgotPasswordCommand(request.Email, request.ResetLinkBaseUrl), cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage
            });
        }

        return Ok();
    }

    /// <summary>
    /// Verifies the password reset token and updates the user's password.
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _resetPasswordHandler.HandleAsync(
            new ResetPasswordCommand(request.Email, request.Token, request.Password), cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = result.ErrorCode,
                Detail = result.ErrorMessage
            });
        }

        return Ok();
    }
}

public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);
public record ForgotPasswordRequest(string Email, string ResetLinkBaseUrl);
public record ResetPasswordRequest(string Email, string Token, string Password);
