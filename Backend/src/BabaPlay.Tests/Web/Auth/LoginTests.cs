using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Moq;
using Xunit;
using BabaPlay.Web.Models;
using BabaPlay.Web.Pages.Auth;
using BabaPlay.Web.Services.Http;
using BabaPlay.Web.Services.State;
using BabaPlay.Web.Services.Storage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Tests.Web.Auth;

public class LoginTests : TestContext
{
    [Fact]
    public void Login_ShouldRenderFormWithEmailAndPasswordInputs()
    {
        RegisterLoginServices(new Mock<IAuthApiService>());

        var cut = RenderComponent<Login>();

        cut.Find("#email-input").Should().NotBeNull();
        cut.Find("#password-input").Should().NotBeNull();
        cut.Find("#login-submit-button").Should().NotBeNull();
    }

    [Fact]
    public async Task Login_WhenCredentialsAreInvalid_ShouldShowErrorMessage()
    {
        var mockAuthService = new Mock<IAuthApiService>();
        mockAuthService
            .Setup(x => x.LoginAsync(It.IsAny<LoginDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthResponseDto?)null);

        RegisterLoginServices(mockAuthService);

        var cut = RenderComponent<Login>();

        cut.Find("#email-input").Change("errado@baba.com");
        cut.Find("#password-input").Change("senha123");
        await cut.Find("#login-form").SubmitAsync();

        cut.Find("#login-error-alert").MarkupMatches(@$"<div id=""login-error-alert"" class=""p-4 rounded-lg bg-rose-500/10 border border-rose-500/30 text-rose-300 text-sm flex items-center gap-2""><svg class=""w-5 h-5 shrink-0 text-rose-400"" fill=""none"" viewBox=""0 0 24 24"" stroke=""currentColor""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z""></path></svg><span>E-mail ou senha incorretos. Verifique suas credenciais.</span></div>");
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ShouldStoreSessionPersistAndNavigateToDashboard()
    {
        var tenantId = Guid.NewGuid();
        var accessToken = CreateJwt("user-123", "admin@baba.com", ["Admin"]);
        var authResponse = new AuthResponseDto(
            accessToken,
            "refresh-token",
            3600,
            "Bearer",
            new AuthTenantMembershipDto(tenantId, "Baba FC", "baba-fc", true, DateTime.UtcNow),
            new List<AuthTenantMembershipDto>
            {
                new(tenantId, "Baba FC", "baba-fc", true, DateTime.UtcNow),
            });

        var mockAuthService = new Mock<IAuthApiService>();
        mockAuthService
            .Setup(x => x.LoginAsync(It.IsAny<LoginDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        var mockStorage = new Mock<IAuthSessionStorage>();
        mockStorage
            .Setup(x => x.SaveAsync(It.IsAny<AuthSessionSnapshot>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var userSessionState = RegisterLoginServices(mockAuthService, mockStorage);

        var cut = RenderComponent<Login>();
        var navManager = Services.GetRequiredService<NavigationManager>();

        cut.Find("#email-input").Change("admin@baba.com");
        cut.Find("#password-input").Change("Senha123");
        await cut.Find("#login-form").SubmitAsync();

        userSessionState.IsAuthenticated.Should().BeTrue();
        userSessionState.JwtToken.Should().Be(accessToken);
        userSessionState.RefreshToken.Should().Be("refresh-token");
        navManager.Uri.Should().EndWith("/");
        mockStorage.Verify(x => x.SaveAsync(It.IsAny<AuthSessionSnapshot>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private UserSessionState RegisterLoginServices(
        Mock<IAuthApiService> mockAuthService,
        Mock<IAuthSessionStorage>? mockStorage = null)
    {
        var userSessionState = new UserSessionState();
        var tenantState = new TenantState();
        var customAuthStateProvider = new CustomAuthStateProvider(userSessionState);
        var storage = mockStorage ?? new Mock<IAuthSessionStorage>();

        storage.Setup(x => x.SaveAsync(It.IsAny<AuthSessionSnapshot>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        storage.Setup(x => x.ClearAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        Services.AddSingleton(mockAuthService.Object);
        Services.AddSingleton(storage.Object);
        Services.AddSingleton(userSessionState);
        Services.AddSingleton(tenantState);
        Services.AddSingleton(customAuthStateProvider);
        Services.AddSingleton<AuthenticationStateProvider>(customAuthStateProvider);
        Services.AddSingleton(new AuthSessionService(
            storage.Object,
            userSessionState,
            tenantState,
            customAuthStateProvider,
            mockAuthService.Object));

        return userSessionState;
    }

    private static string CreateJwt(string userId, string email, IReadOnlyList<string> roles)
    {
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"none\",\"typ\":\"JWT\"}"))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var payloadJson = $$"""
            {"sub":"{{userId}}","email":"{{email}}","role":["{{string.Join("\",\"", roles)}}"]}
            """;
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadJson))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return $"{header}.{payload}.signature";
    }
}
