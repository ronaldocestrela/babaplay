using System.Threading;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Moq;
using Xunit;
using BabaPlay.Web.Models;
using BabaPlay.Web.Pages.Tenant;
using BabaPlay.Web.Services.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Tests.Web.Tenant;

public class AcceptInviteTests : TestContext
{
    [Fact]
    public void AcceptInvite_WhenTokenInvalid_ShouldShowInvalidState()
    {
        // Arrange
        var mockTenantService = new Mock<ITenantApiService>();
        mockTenantService
            .Setup(x => x.ValidateInviteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InviteValidationDto(false, "", "", "", "Token inválido."));

        Services.AddSingleton(mockTenantService.Object);

        var navManager = Services.GetRequiredService<NavigationManager>();
        navManager.NavigateTo("http://localhost/tenant/accept-invite?Token=invalid-token");

        // Act
        var cut = RenderComponent<AcceptInvite>();

        // Assert
        cut.Markup.Should().Contain("Convite Inválido ou Expirado");
    }

    [Fact]
    public void AcceptInvite_WhenTokenValid_ShouldShowInviteDetailsAndAcceptButton()
    {
        // Arrange
        var mockTenantService = new Mock<ITenantApiService>();
        mockTenantService
            .Setup(x => x.ValidateInviteAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InviteValidationDto(true, "Baba do Resenha", "atleta@baba.com", "Atleta", null));

        Services.AddSingleton(mockTenantService.Object);

        var navManager = Services.GetRequiredService<NavigationManager>();
        navManager.NavigateTo("http://localhost/tenant/accept-invite?Token=valid-token");

        // Act
        var cut = RenderComponent<AcceptInvite>();

        // Assert
        cut.Markup.Should().Contain("Você foi convidado!");
        cut.Markup.Should().Contain("Baba do Resenha");
        cut.Find("#accept-invite-submit").Should().NotBeNull();
    }
}
