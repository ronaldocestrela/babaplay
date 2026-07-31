using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Moq;
using Xunit;
using BabaPlay.Web.Components.Auth;
using BabaPlay.Web.Services.Http;
using BabaPlay.Web.Services.State;
using BabaPlay.Web.Services.Storage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Tests.Web.Auth;

public class AuthSessionInitializerTests : TestContext
{
    [Fact]
    public async Task AuthSessionInitializer_ShouldRestoreSessionBeforeRenderingChildContent()
    {
        // Arrange
        var tenantId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var snapshot = new AuthSessionSnapshot
        {
            AccessToken = CreateJwt("user-1", "user@baba.com", ["Admin"], 3600),
            RefreshToken = "refresh-token",
            UserId = "user-1",
            Email = "user@baba.com",
            FullName = "User Baba",
            Roles = ["Admin"],
            TenantId = tenantId,
            TenantName = "Baba FC",
            TenantSlug = "baba-fc",
        };

        var storage = new Mock<IAuthSessionStorage>();
        storage.Setup(x => x.LoadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(snapshot);
        storage.Setup(x => x.SaveAsync(It.IsAny<AuthSessionSnapshot>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        storage.Setup(x => x.ClearAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var userSessionState = new UserSessionState();
        var tenantState = new TenantState();
        var authStateProvider = new CustomAuthStateProvider(userSessionState);
        var authApiService = new Mock<IAuthApiService>();

        Services.AddSingleton(storage.Object);
        Services.AddSingleton(userSessionState);
        Services.AddSingleton(tenantState);
        Services.AddSingleton(authStateProvider);
        Services.AddSingleton<AuthenticationStateProvider>(authStateProvider);
        Services.AddSingleton(authApiService.Object);
        Services.AddSingleton(new AuthSessionService(
            storage.Object,
            userSessionState,
            tenantState,
            authStateProvider,
            authApiService.Object));

        // Act
        var cut = RenderComponent<AuthSessionInitializer>(parameters =>
            parameters.AddChildContent("<p id=\"app-ready\">ready</p>"));

        cut.WaitForAssertion(() => cut.Find("#app-ready").Should().NotBeNull());

        // Assert
        userSessionState.IsAuthenticated.Should().BeTrue();
        tenantState.CurrentTenantSlug.Should().Be("baba-fc");
    }

    private static string CreateJwt(string userId, string email, IReadOnlyList<string> roles, int expiresInSeconds)
    {
        var exp = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds).ToUnixTimeSeconds();
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"none\",\"typ\":\"JWT\"}"))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var payloadJson = $$"""
            {"sub":"{{userId}}","email":"{{email}}","role":["{{string.Join("\",\"", roles)}}"],"exp":{{exp}}}
            """;
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadJson))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return $"{header}.{payload}.signature";
    }
}
