namespace BabaPlay.Application.Interfaces;

public interface ITenantLogoStorageService
{
    Task<TenantLogoStoredFile> SaveAsync(TenantLogoSaveRequest request, CancellationToken ct = default);
}

public sealed record TenantLogoSaveRequest(
    Guid TenantId,
    string FileName,
    string ContentType,
    byte[] Content);

public sealed record TenantLogoStoredFile(
    string StoragePath,
    string ContentType,
    long SizeBytes);