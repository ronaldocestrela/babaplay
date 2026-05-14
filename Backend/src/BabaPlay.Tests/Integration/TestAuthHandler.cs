using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BabaPlay.Tests.Integration;

/// <summary>
/// Test-only authentication handler that always succeeds, bypassing JWT validation.
/// Used in integration tests to isolate tenant/business logic from authentication concerns.
/// </summary>
public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestAuthScheme";
    public const string TestUserId = "test-user-id";
    public const string TestUserEmail = "test@babaplay.com";
    public const string UserIdHeader = "X-Test-UserId";
    public const string UserEmailHeader = "X-Test-UserEmail";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers.TryGetValue(UserIdHeader, out var userIdValues)
            ? userIdValues.ToString()
            : TestUserId;

        var email = Request.Headers.TryGetValue(UserEmailHeader, out var emailValues)
            ? emailValues.ToString()
            : TestUserEmail;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, email),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
