using System;
using System.Reflection;
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
        cut.Find("#logo-file").Should().NotBeNull();
        cut.Find("#admin-email").Should().NotBeNull();
        cut.Find("#admin-password").Should().NotBeNull();
        cut.Find("#street-input").Should().NotBeNull();
        cut.Find("#number-input").Should().NotBeNull();
        cut.Find("#zipcode-input").Should().NotBeNull();
        cut.Find("#latitude-input").Should().NotBeNull();
        cut.Find("#longitude-input").Should().NotBeNull();
        cut.Find("#city-input").Should().NotBeNull();
        cut.Find("#state-input").Should().NotBeNull();
        cut.Find("#register-association-submit").Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterAssociation_WhenSubmittedWithAddress_ShouldCallCreateTenantAsync()
    {
        // Arrange
        var mockTenantService = new Mock<ITenantApiService>();
        CreateTenantDto? capturedDto = null;

        mockTenantService
            .Setup(x => x.CreateTenantAsync(It.IsAny<CreateTenantDto>(), It.IsAny<IBrowserFile>(), It.IsAny<CancellationToken>()))
            .Callback<CreateTenantDto, IBrowserFile?, CancellationToken>((dto, _, _) => capturedDto = dto)
            .ReturnsAsync((new TenantSummaryDto(Guid.NewGuid(), "Baba FC", "baba-fc", null, "Owner"), null));

        Services.AddSingleton(mockTenantService.Object);

        var cut = RenderComponent<RegisterAssociation>();
        SetSelectedLogoFile(cut.Instance, CreateMockLogoFile());

        cut.Find("#tenant-name").Change("Baba FC");
        cut.Find("#tenant-slug").Change("baba-fc");
        cut.Find("#admin-email").Change("admin@baba.com");
        cut.Find("#admin-password").Change("Senha123");
        cut.Find("#street-input").Change("Rua das Palmeiras");
        cut.Find("#number-input").Change("100");
        cut.Find("#city-input").Change("Salvador");
        cut.Find("#state-input").Change("BA");
        cut.Find("#zipcode-input").Change("40000-000");
        cut.Find("#latitude-input").Change("-12.971400");
        cut.Find("#longitude-input").Change("-38.501400");

        // Act
        await cut.Find("#register-association-form").SubmitAsync();

        // Assert
        mockTenantService.Verify(
            x => x.CreateTenantAsync(It.IsAny<CreateTenantDto>(), It.IsAny<IBrowserFile>(), It.IsAny<CancellationToken>()),
            Times.Once);

        capturedDto.Should().NotBeNull();
        capturedDto!.Street.Should().Be("Rua das Palmeiras");
        capturedDto.Number.Should().Be("100");
        capturedDto.ZipCode.Should().Be("40000-000");
        capturedDto.AssociationLatitude.Should().BeApproximately(-12.9714, 0.0001);
        capturedDto.AssociationLongitude.Should().BeApproximately(-38.5014, 0.0001);
    }

    private static void SetSelectedLogoFile(RegisterAssociation component, IBrowserFile logoFile)
    {
        var field = typeof(RegisterAssociation).GetField("selectedLogoFile", BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(component, logoFile);
    }

    private static IBrowserFile CreateMockLogoFile()
    {
        var mockFile = new Mock<IBrowserFile>();
        mockFile.SetupGet(x => x.Name).Returns("logo.png");
        mockFile.SetupGet(x => x.ContentType).Returns("image/png");
        mockFile.SetupGet(x => x.Size).Returns(1024);
        mockFile
            .Setup(x => x.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47 }));
        return mockFile.Object;
    }
}
