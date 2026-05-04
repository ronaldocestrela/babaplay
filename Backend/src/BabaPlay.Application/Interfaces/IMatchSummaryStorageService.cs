namespace BabaPlay.Application.Interfaces;

public interface IMatchSummaryStorageService
{
    Task<MatchSummaryStoredFile> SaveAsync(MatchSummaryFileSaveRequest request, CancellationToken ct = default);

    Task<byte[]?> ReadAsync(string storagePath, CancellationToken ct = default);
}

public sealed record MatchSummaryFileSaveRequest(
    Guid TenantId,
    Guid MatchId,
    byte[] Content);

public sealed record MatchSummaryStoredFile(
    string StoragePath,
    string FileName,
    string ContentType,
    long SizeBytes);
