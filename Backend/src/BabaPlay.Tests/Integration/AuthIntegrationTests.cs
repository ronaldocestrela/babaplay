using System.Net;
using System.Net.Http.Json;
using BabaPlay.Api.Controllers;
using BabaPlay.Application.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Tests.Integration;

/// <summary>
/// End-to-end integration tests for Phase 1 auth endpoints.
/// Covers the three mandatory roadmap scenarios: valid login, invalid login, refresh token.
/// </summary>
public class AuthIntegrationTests : IClassFixture<AuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(AuthWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── Login ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task POST_Login_ValidCredentials_ShouldReturn200WithTokens()
    {
        // Arrange
        var request = new LoginRequest(AuthWebApplicationFactory.TestUserEmail, AuthWebApplicationFactory.TestUserPassword);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.ExpiresIn.Should().BeGreaterThan(0);
        body.TokenType.Should().Be("Bearer");
        body.PrimaryTenant.Should().NotBeNull();
        body.PrimaryTenant!.Slug.Should().Be(AuthWebApplicationFactory.TestTenantSlug);
        body.Tenants.Should().NotBeNull();
        body.Tenants!.Should().ContainSingle(t => t.Slug == AuthWebApplicationFactory.TestTenantSlug && t.IsOwner);
    }

    [Fact]
    public async Task POST_Login_InvalidPassword_ShouldReturn401()
    {
        // Arrange — valid e-mail, wrong password
        var request = new LoginRequest(AuthWebApplicationFactory.TestUserEmail, "WrongPassword@999");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be("INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task POST_Login_UnknownEmail_ShouldReturn401()
    {
        // Arrange — prevents user enumeration: same status as wrong password
        var request = new LoginRequest("nonexistent@babaplay.com", "AnyPassword@123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be("INVALID_CREDENTIALS");
    }

    // ── Refresh Token ──────────────────────────────────────────────────────

    [Fact]
    public async Task POST_RefreshToken_ValidToken_ShouldReturn200WithNewTokenPair()
    {
        // Arrange — use pre-seeded valid token
        var request = new RefreshTokenRequest(AuthWebApplicationFactory.ValidRefreshToken);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
        // New refresh token must differ from the consumed one (rotation)
        body.RefreshToken.Should().NotBe(AuthWebApplicationFactory.ValidRefreshToken);
    }

    [Fact]
    public async Task POST_RefreshToken_ExpiredToken_ShouldReturn401()
    {
        // Arrange — pre-seeded expired token
        var request = new RefreshTokenRequest(AuthWebApplicationFactory.ExpiredRefreshToken);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be("TOKEN_EXPIRED");
    }

    [Fact]
    public async Task POST_RefreshToken_InvalidToken_ShouldReturn401()
    {
        // Arrange
        var request = new RefreshTokenRequest("completely-unknown-token-xxxxxx");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be("INVALID_TOKEN");
    }

    [Fact]
    public async Task POST_RefreshToken_ConsumedToken_ShouldReturn401_AfterRotation()
    {
        // Arrange — first get a fresh valid token via login
        var loginRequest = new LoginRequest(AuthWebApplicationFactory.TestUserEmail, AuthWebApplicationFactory.TestUserPassword);
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        var freshToken = loginBody!.RefreshToken;

        // Use the token once (consumes + rotates)
        await _client.PostAsJsonAsync("/api/v1/auth/refresh-token", new RefreshTokenRequest(freshToken));

        // Act — attempt to reuse the same (now revoked) token
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh-token", new RefreshTokenRequest(freshToken));

        // Assert — revoked token must be rejected
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

}
