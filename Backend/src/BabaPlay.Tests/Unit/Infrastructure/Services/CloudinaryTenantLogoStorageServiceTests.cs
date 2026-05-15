using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Services;
using BabaPlay.Infrastructure.Settings;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace BabaPlay.Tests.Unit.Infrastructure.Services;

public class CloudinaryTenantLogoStorageServiceTests
{
    [Fact]
    public async Task SaveAsync_UploadSuccess_ShouldReturnSecureUrlAsStoragePath()
    {
        var uploader = new Mock<ICloudinaryImageUploader>();
        uploader
            .Setup(x => x.UploadAsync(It.IsAny<CloudinaryImageUploadRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CloudinaryImageUploadResult(
                true,
                "https://res.cloudinary.com/demo/image/upload/v1/tenant-logos/tenant/logo.png",
                "tenant-logos/tenant/logo",
                2048,
                null));

        var options = Options.Create(new TenantLogoStorageSettings
        {
            Provider = TenantLogoStorageProviders.Cloudinary,
            Cloudinary = new CloudinaryTenantLogoStorageSettings
            {
                Folder = "tenant-logos",
            },
        });

        ITenantLogoStorageService sut = new CloudinaryTenantLogoStorageService(uploader.Object, options);

        var result = await sut.SaveAsync(new TenantLogoSaveRequest(
            Guid.NewGuid(),
            "logo.png",
            "image/png",
            [1, 2, 3]));

        result.StoragePath.Should().Be("https://res.cloudinary.com/demo/image/upload/v1/tenant-logos/tenant/logo.png");
        result.ContentType.Should().Be("image/png");
        result.SizeBytes.Should().Be(2048);
    }

    [Fact]
    public async Task SaveAsync_UploadFails_ShouldThrowInvalidOperationException()
    {
        var uploader = new Mock<ICloudinaryImageUploader>();
        uploader
            .Setup(x => x.UploadAsync(It.IsAny<CloudinaryImageUploadRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CloudinaryImageUploadResult(
                false,
                null,
                null,
                0,
                "Invalid signature"));

        var options = Options.Create(new TenantLogoStorageSettings
        {
            Provider = TenantLogoStorageProviders.Cloudinary,
            Cloudinary = new CloudinaryTenantLogoStorageSettings
            {
                Folder = "tenant-logos",
            },
        });

        ITenantLogoStorageService sut = new CloudinaryTenantLogoStorageService(uploader.Object, options);

        var act = async () => await sut.SaveAsync(new TenantLogoSaveRequest(
            Guid.NewGuid(),
            "logo.png",
            "image/png",
            [1, 2, 3]));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cloudinary tenant logo upload failed:*");
    }
}
