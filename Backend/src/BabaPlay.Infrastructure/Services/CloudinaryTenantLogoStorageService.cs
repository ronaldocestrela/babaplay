using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace BabaPlay.Infrastructure.Services;

public sealed class CloudinaryTenantLogoStorageService : ITenantLogoStorageService
{
    private readonly ICloudinaryImageUploader _cloudinaryImageUploader;
    private readonly string _baseFolder;

    public CloudinaryTenantLogoStorageService(
        ICloudinaryImageUploader cloudinaryImageUploader,
        IOptions<TenantLogoStorageSettings> options)
    {
        _cloudinaryImageUploader = cloudinaryImageUploader;

        var configuredFolder = options.Value.Cloudinary.Folder?.Trim();
        _baseFolder = string.IsNullOrWhiteSpace(configuredFolder)
            ? "tenant-logos"
            : configuredFolder.Trim('/');
    }

    public async Task<TenantLogoStoredFile> SaveAsync(TenantLogoSaveRequest request, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(request.FileName);
        var safeExtension = string.IsNullOrWhiteSpace(extension)
            ? string.Empty
            : extension.ToLowerInvariant();

        var publicId = $"{Guid.NewGuid():N}{safeExtension}";
        var folder = $"{_baseFolder}/{request.TenantId:N}";

        var uploadResult = await _cloudinaryImageUploader.UploadAsync(new CloudinaryImageUploadRequest(
            request.FileName,
            request.Content,
            folder,
            publicId), ct);

        if (!uploadResult.IsSuccess || string.IsNullOrWhiteSpace(uploadResult.SecureUrl))
        {
            var detail = string.IsNullOrWhiteSpace(uploadResult.ErrorMessage)
                ? "Unknown Cloudinary upload error."
                : uploadResult.ErrorMessage;
            throw new InvalidOperationException($"Cloudinary tenant logo upload failed: {detail}");
        }

        return new TenantLogoStoredFile(
            uploadResult.SecureUrl,
            request.ContentType,
            uploadResult.Bytes > 0 ? uploadResult.Bytes : request.Content.LongLength);
    }
}
