using BabaPlay.Modules.Identity.Services;
using BabaPlay.SharedKernel.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Modules.Identity.Controllers;

[AllowAnonymous]
[Route("api/[controller]")]
public sealed class AuthController : BaseController
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth) => _auth = auth;

    public sealed record RegisterRequest(string Name, string Email, string Password, UserType UserType = UserType.Associate);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest body, CancellationToken ct) =>
        FromResult(await _auth.RegisterAsync(body.Name, body.Email, body.Password, body.UserType, ct));

    public sealed record RegisterWithInvitationRequest(string InvitationToken, string Name, string? Email, string Password);

    [HttpPost("register-with-invitation")]
    public async Task<IActionResult> RegisterWithInvitation([FromBody] RegisterWithInvitationRequest body, CancellationToken ct) =>
        FromResult(await _auth.RegisterWithInvitationAsync(body.InvitationToken, body.Name, body.Email, body.Password, ct));

    public sealed record LoginRequest(string Email, string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest body, CancellationToken ct) =>
        FromResult(await _auth.LoginAsync(body.Email, body.Password, ct));
}
