using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

/// <summary>
/// Represents a tenant-scoped position that can be associated to players.
/// </summary>
public sealed class Position : EntityBase
{
    public Guid TenantId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string NormalizedCode { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private Position() { }

    public static Position Create(Guid tenantId, string code, string name, string? description)
    {
        if (tenantId == Guid.Empty)
            throw new ValidationException("TenantId", "TenantId is required.");

        if (string.IsNullOrWhiteSpace(code))
            throw new ValidationException("Code", "Position code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name", "Position name is required.");

        var trimmedCode = code.Trim();
        var trimmedName = name.Trim();

        return new Position
        {
            TenantId = tenantId,
            Code = trimmedCode,
            NormalizedCode = NormalizeCode(trimmedCode),
            Name = trimmedName,
            Description = description?.Trim(),
            IsActive = true,
        };
    }

    public void Update(string code, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ValidationException("Code", "Position code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name", "Position name is required.");

        Code = code.Trim();
        NormalizedCode = NormalizeCode(Code);
        Name = name.Trim();
        Description = description?.Trim();
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
