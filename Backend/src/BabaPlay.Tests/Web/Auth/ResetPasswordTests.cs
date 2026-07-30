using System.Threading;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Moq;
using Xunit;
using BabaPlay.Web.Models;
using BabaPlay.Web.Pages.Auth;
using BabaPlay.Web.Services.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Tests.Web.Auth;

public class ResetPasswordTests : TestContext
{
    [Fact]
    public void ResetPassword_ShouldPopulateParametersFromQuery()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthApiService>();
        Services.AddSingleton(mockAuthService.Object);

        var navManager = Services.GetRequiredService<NavigationManager>();
        navManager.NavigateTo("http://localhost/auth/reset-password?Token=sample-token-123&Email=atleta@baba.com");

        // Act
        var cut = RenderComponent<ResetPassword>();

        // Assert
        cut.Find("#email-input").GetAttribute("value").Should().Be("atleta@baba.com");
    }

    [Fact]
    public async Task ResetPassword_WhenSuccess_ShouldShowSuccessMessage()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthApiService>();
        mockAuthService
            .Setup(x => x.ResetPasswordAsync(It.IsAny<ResetPasswordDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Services.AddSingleton(mockAuthService.Object);

        var navManager = Services.GetRequiredService<NavigationManager>();
        navManager.NavigateTo("http://localhost/auth/reset-password?Token=valid-token&Email=atleta@baba.com");

        var cut = RenderComponent<ResetPassword>();

        // Act
        cut.Find("#password-input").Change("NovaSenha123");
        cut.Find("#confirm-password-input").Change("NovaSenha123");
        await cut.Find("#reset-password-form").SubmitAsync();

        // Assert
        cut.Find("#reset-success-alert").Should().NotBeNull();
    }
}
