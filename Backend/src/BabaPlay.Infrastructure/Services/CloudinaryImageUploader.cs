using BabaPlay.Infrastructure.Settings;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace BabaPlay.Infrastructure.Services;

public interface ICloudinaryImageUploader
{
    Task<CloudinaryImageUploadResult> UploadAsync(CloudinaryImageUploadRequest request, CancellationToken ct = default);
}

public sealed record CloudinaryImageUploadRequest(
    string FileName,
    byte[] Content,
    string Folder,
    string PublicId);

public sealed record CloudinaryImageUploadResult(
    bool IsSuccess,
    string? SecureUrl,
    string? PublicId,
    long Bytes,
    string? ErrorMessage);

public sealed class CloudinaryImageUploader : ICloudinaryImageUploader
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryImageUploader(IOptions<TenantLogoStorageSettings> options)
    {
        var cloudinarySettings = options.Value.Cloudinary;

        var cloudName = ResolveWithEnvironmentFallback(cloudinarySettings.CloudName, "CLOUDINARY_CLOUD_NAME");
        var apiKey = ResolveWithEnvironmentFallback(cloudinarySettings.ApiKey, "CLOUDINARY_API_KEY");
        var apiSecret = ResolveWithEnvironmentFallback(cloudinarySettings.ApiSecret, "CLOUDINARY_API_SECRET");

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account)
        {
            Api =
            {
                Secure = true,
            },
        };
    }

    public async Task<CloudinaryImageUploadResult> UploadAsync(CloudinaryImageUploadRequest request, CancellationToken ct = default)
    {
        using var stream = new MemoryStream(request.Content, writable: false);

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(request.FileName, stream),
            Folder = request.Folder,
            PublicId = request.PublicId,
            Overwrite = true,
            UniqueFilename = false,
            UseFilename = false,
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        return new CloudinaryImageUploadResult(
            string.IsNullOrWhiteSpace(uploadResult.Error?.Message),
            uploadResult.SecureUrl?.AbsoluteUri,
            uploadResult.PublicId,
            uploadResult.Bytes,
            uploadResult.Error?.Message);
    }

    private static string ResolveWithEnvironmentFallback(string value, string environmentVariable)
    {
        if (!string.IsNullOrWhiteSpace(value))
            return value.Trim();

        var fromEnvironment = Environment.GetEnvironmentVariable(environmentVariable);
        return string.IsNullOrWhiteSpace(fromEnvironment) ? string.Empty : fromEnvironment.Trim();
    }
}
