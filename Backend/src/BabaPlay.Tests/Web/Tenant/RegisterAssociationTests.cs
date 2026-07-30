using System.Threading;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Moq;
using Xunit;
using BabaPlay.Web.Models;
using BabaPlay.Web.Pages.Tenant;
using BabaPlay.Web.Services.Http;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Tests.Web.Tenant;

public class RegisterAssociationTests : TestContext
{
    [Fact]
    public void RegisterAssociation_ShouldRenderAllFormInputs()
    {
        // Arrange
        var mockTenantService = new Mock<ITenantApiService>();
        Services.AddSingleton(mockTenantService.Object);

        // Act
        var cut = RenderComponent<RegisterAssociation>();

        // Assert
        cut.Find("#tenant-name").Should().NotBeNull();
        cut.Find("#tenant-slug").Should().NotBeNull();
        cut.Find("#admin-email").Should().NotBeNull();
        cut.Find("#admin-password").Should().NotBeNull();
        cut.Find("#city-input").Should().NotBeNull();
        cut.Find("#state-input").Should().NotBeNull();
        cut.Find("#register-association-submit").Should().NotBeNull();
    }
}
