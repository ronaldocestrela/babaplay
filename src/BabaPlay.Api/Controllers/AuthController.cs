using BabaPlay.Application.Commands.Auth;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly ICommandHandler<LoginCommand, Result<AuthResponse>> _loginHandler;
    private readonly ICommandHandler<RefreshTokenCommand, Result<AuthResponse>> _refreshTokenHandler;

    public AuthController(
        ICommandHandler<LoginCommand, Result<AuthResponse>> loginHandler,
        ICommandHandler<RefreshTokenCommand, Result<AuthResponse>> refreshTokenHandler)
    {
        _loginHandler = loginHandler;
        _refreshTokenHandler = refreshTokenHandler;
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
}

public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);
