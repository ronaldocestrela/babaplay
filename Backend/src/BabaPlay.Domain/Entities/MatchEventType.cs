using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

/// <summary>
/// Represents a tenant-configurable match event type with scoring points.
/// </summary>
public sealed class MatchEventType : EntityBase
{
    public Guid TenantId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string NormalizedCode { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public int Points { get; private set; }
    public bool IsSystemDefault { get; private set; }
    public bool IsActive { get; private set; }

    private MatchEventType() { }

    public static MatchEventType Create(Guid tenantId, string code, string name, int points, bool isSystemDefault)
    {
        if (tenantId == Guid.Empty)
            throw new ValidationException("TenantId", "TenantId is required.");

        if (string.IsNullOrWhiteSpace(code))
            throw new ValidationException("Code", "Code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name", "Name is required.");

        var trimmedCode = code.Trim();

        return new MatchEventType
        {
            TenantId = tenantId,
            Code = trimmedCode,
            NormalizedCode = NormalizeCode(trimmedCode),
            Name = name.Trim(),
            Points = points,
            IsSystemDefault = isSystemDefault,
            IsActive = true,
        };
    }

    public void Update(string code, string name, int points)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ValidationException("Code", "Code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name", "Name is required.");

        var trimmedCode = code.Trim();

        Code = trimmedCode;
        NormalizedCode = NormalizeCode(trimmedCode);
        Name = name.Trim();
        Points = points;
        MarkUpdated();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        MarkUpdated();
    }

    private static string NormalizeCode(string code)
        => code.Trim().ToUpperInvariant();
}
