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
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Tests.Web.Auth;

public class LoginTests : TestContext
{
    [Fact]
    public void Login_ShouldRenderFormWithEmailAndPasswordInputs()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthApiService>();
        var userSessionState = new UserSessionState();
        var customAuthStateProvider = new CustomAuthStateProvider(userSessionState);

        Services.AddSingleton(mockAuthService.Object);
        Services.AddSingleton(userSessionState);
        Services.AddSingleton(customAuthStateProvider);
        Services.AddSingleton<AuthenticationStateProvider>(customAuthStateProvider);

        // Act
        var cut = RenderComponent<Login>();

        // Assert
        cut.Find("#email-input").Should().NotBeNull();
        cut.Find("#password-input").Should().NotBeNull();
        cut.Find("#login-submit-button").Should().NotBeNull();
    }

    [Fact]
    public async Task Login_WhenCredentialsAreInvalid_ShouldShowErrorMessage()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthApiService>();
        mockAuthService
            .Setup(x => x.LoginAsync(It.IsAny<LoginDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthResponseDto?)null);

        var userSessionState = new UserSessionState();
        var customAuthStateProvider = new CustomAuthStateProvider(userSessionState);

        Services.AddSingleton(mockAuthService.Object);
        Services.AddSingleton(userSessionState);
        Services.AddSingleton(customAuthStateProvider);
        Services.AddSingleton<AuthenticationStateProvider>(customAuthStateProvider);

        var cut = RenderComponent<Login>();

        // Act
        cut.Find("#email-input").Change("errado@baba.com");
        cut.Find("#password-input").Change("senha123");
        await cut.Find("#login-form").SubmitAsync();

        // Assert
        cut.Find("#login-error-alert").MarkupMatches(@$"<div id=""login-error-alert"" class=""p-4 rounded-lg bg-rose-500/10 border border-rose-500/30 text-rose-300 text-sm flex items-center gap-2""><svg class=""w-5 h-5 shrink-0 text-rose-400"" fill=""none"" viewBox=""0 0 24 24"" stroke=""currentColor""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z""></path></svg><span>E-mail ou senha incorretos. Verifique suas credenciais.</span></div>");
    }
}
