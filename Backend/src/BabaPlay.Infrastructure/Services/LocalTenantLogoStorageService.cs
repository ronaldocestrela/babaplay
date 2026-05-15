using BabaPlay.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using BabaPlay.Infrastructure.Settings;

namespace BabaPlay.Infrastructure.Services;

public sealed class LocalTenantLogoStorageService : ITenantLogoStorageService
{
    private readonly string _storageRoot;

    public LocalTenantLogoStorageService(
        IHostEnvironment hostEnvironment,
        IOptions<TenantLogoStorageSettings> storageOptions)
    {
        var configuredRootPath = storageOptions.Value.LocalRootPath?.Trim();
        var rootPath = string.IsNullOrWhiteSpace(configuredRootPath) ? "storage" : configuredRootPath;

        _storageRoot = Path.IsPathRooted(rootPath)
            ? rootPath
            : Path.Combine(hostEnvironment.ContentRootPath, rootPath);

        _storageRoot = Path.GetFullPath(_storageRoot);
    }

    public async Task<TenantLogoStoredFile> SaveAsync(TenantLogoSaveRequest request, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(request.FileName);
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".bin";

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var relativePath = Path.Combine("tenant-logos", request.TenantId.ToString("N"), fileName);
        var normalizedPath = relativePath.Replace('\\', '/');
        var fullPath = Path.Combine(_storageRoot, normalizedPath);

        var directory = Path.GetDirectoryName(fullPath)
            ?? throw new InvalidOperationException("Unable to determine storage directory for tenant logo.");
        Directory.CreateDirectory(directory);

        await File.WriteAllBytesAsync(fullPath, request.Content, ct);

        return new TenantLogoStoredFile(
            normalizedPath,
            request.ContentType,
            request.Content.LongLength);
    }
}
