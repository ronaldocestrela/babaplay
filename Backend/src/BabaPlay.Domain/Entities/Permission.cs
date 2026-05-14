using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

/// <summary>
/// Represents a permission that can be assigned to one or more roles.
/// </summary>
public sealed class Permission : EntityBase
{
    public string Code { get; private set; } = string.Empty;
    public string NormalizedCode { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsSystem { get; private set; }

    private Permission() { }

    public static Permission Create(string code, string? description, bool isSystem = true)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ValidationException("Code", "Permission code is required.");

        var trimmedCode = code.Trim();

        return new Permission
        {
            Code = trimmedCode,
            NormalizedCode = NormalizeCode(trimmedCode),
            Description = description?.Trim(),
            IsSystem = isSystem,
        };
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
        MarkUpdated();
    }

    private static string NormalizeCode(string code)
        => code.Trim().ToUpperInvariant();
}
