using BabaPlay.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using BabaPlay.Infrastructure.Settings;

namespace BabaPlay.Infrastructure.Services;

public sealed class LocalMatchSummaryStorageService : IMatchSummaryStorageService
{
    private readonly string _storageRoot;

    public LocalMatchSummaryStorageService(
        IHostEnvironment hostEnvironment,
        IOptions<MatchSummaryStorageSettings> storageOptions)
    {
        var configuredRootPath = storageOptions.Value.RootPath?.Trim();
        var rootPath = string.IsNullOrWhiteSpace(configuredRootPath) ? "storage" : configuredRootPath;

        _storageRoot = Path.IsPathRooted(rootPath)
            ? rootPath
            : Path.Combine(hostEnvironment.ContentRootPath, rootPath);

        _storageRoot = Path.GetFullPath(_storageRoot);
    }

    public async Task<MatchSummaryStoredFile> SaveAsync(MatchSummaryFileSaveRequest request, CancellationToken ct = default)
    {
        var fileName = $"{request.MatchId:N}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var relativePath = Path.Combine("match-summaries", request.TenantId.ToString("N"), fileName);
        var normalizedPath = relativePath.Replace('\\', '/');
        var fullPath = Path.Combine(_storageRoot, normalizedPath);

        var directory = Path.GetDirectoryName(fullPath)
            ?? throw new InvalidOperationException("Unable to determine storage directory for match summary.");
        Directory.CreateDirectory(directory);

        await File.WriteAllBytesAsync(fullPath, request.Content, ct);

        return new MatchSummaryStoredFile(
            normalizedPath,
            fileName,
            "application/pdf",
            request.Content.LongLength);
    }

    public async Task<byte[]?> ReadAsync(string storagePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
            return null;

        var fullPath = Path.GetFullPath(
            Path.Combine(_storageRoot, storagePath.Replace('/', Path.DirectorySeparatorChar)));

        var rootWithSeparator = _storageRoot.EndsWith(Path.DirectorySeparatorChar)
            ? _storageRoot
            : _storageRoot + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.Ordinal))
            return null;

        if (!File.Exists(fullPath))
            return null;

        return await File.ReadAllBytesAsync(fullPath, ct);
    }
}
