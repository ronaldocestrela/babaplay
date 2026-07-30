using System.Threading;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Moq;
using Xunit;
using BabaPlay.Web.Models;
using BabaPlay.Web.Pages.Auth;
using BabaPlay.Web.Services.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Tests.Web.Auth;

public class ForgotPasswordTests : TestContext
{
    [Fact]
    public void ForgotPassword_ShouldRenderEmailInputAndSubmitButton()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthApiService>();
        Services.AddSingleton(mockAuthService.Object);

        // Act
        var cut = RenderComponent<ForgotPassword>();

        // Assert
        cut.Find("#email-input").Should().NotBeNull();
        cut.Find("#forgot-submit-button").Should().NotBeNull();
    }

    [Fact]
    public async Task ForgotPassword_WhenSuccess_ShouldShowSuccessAlert()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthApiService>();
        mockAuthService
            .Setup(x => x.ForgotPasswordAsync(It.IsAny<ForgotPasswordDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Services.AddSingleton(mockAuthService.Object);

        var cut = RenderComponent<ForgotPassword>();

        // Act
        cut.Find("#email-input").Change("atleta@baba.com");
        await cut.Find("#forgot-password-form").SubmitAsync();

        // Assert
        cut.Find("#forgot-success-alert").Should().NotBeNull();
    }
}
