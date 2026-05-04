using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

/// <summary>
/// Represents a generated PDF match summary for a tenant match.
/// </summary>
public sealed class MatchSummary : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid MatchId { get; private set; }
    public string StoragePath { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public DateTime GeneratedAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    private MatchSummary() { }

    public static MatchSummary Create(
        Guid tenantId,
        Guid matchId,
        string storagePath,
        string fileName,
        string contentType,
        long sizeBytes)
    {
        if (tenantId == Guid.Empty)
            throw new ValidationException("TenantId", "TenantId is required.");

        if (matchId == Guid.Empty)
            throw new ValidationException("MatchId", "MatchId is required.");

        if (string.IsNullOrWhiteSpace(storagePath))
            throw new ValidationException("StoragePath", "StoragePath is required.");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ValidationException("FileName", "FileName is required.");

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ValidationException("ContentType", "ContentType is required.");

        if (sizeBytes <= 0)
            throw new ValidationException("SizeBytes", "SizeBytes must be greater than zero.");

        return new MatchSummary
        {
            TenantId = tenantId,
            MatchId = matchId,
            StoragePath = storagePath.Trim(),
            FileName = fileName.Trim(),
            ContentType = contentType.Trim(),
            SizeBytes = sizeBytes,
            GeneratedAtUtc = DateTime.UtcNow,
            IsActive = true,
        };
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        MarkUpdated();
    }
}
