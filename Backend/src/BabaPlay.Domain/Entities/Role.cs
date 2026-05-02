using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

/// <summary>
/// Represents a tenant-scoped role used for authorization.
/// </summary>
public sealed class Role : EntityBase
{
    private readonly List<RolePermission> _permissions = [];

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

    private Role() { }

    public static Role Create(Guid tenantId, string name, string? description)
    {
        if (tenantId == Guid.Empty)
            throw new ValidationException("TenantId", "TenantId is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name", "Role name is required.");

        var trimmedName = name.Trim();

        return new Role
        {
            TenantId = tenantId,
            Name = trimmedName,
            NormalizedName = NormalizeName(trimmedName),
            Description = description?.Trim(),
            IsActive = true,
        };
    }

    public void Rename(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name", "Role name is required.");

        Name = name.Trim();
        NormalizedName = NormalizeName(Name);
        Description = description?.Trim();
        MarkUpdated();
    }

    public void AddPermission(Guid permissionId)
    {
        if (permissionId == Guid.Empty)
            throw new ValidationException("PermissionId", "PermissionId is required.");

        if (_permissions.Any(p => p.PermissionId == permissionId))
            return;

        _permissions.Add(RolePermission.Create(Id, permissionId));
        MarkUpdated();
    }

    public void RemovePermission(Guid permissionId)
    {
        var existing = _permissions.FirstOrDefault(p => p.PermissionId == permissionId);
        if (existing is null)
            return;

        _permissions.Remove(existing);
        MarkUpdated();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        MarkUpdated();
    }

    private static string NormalizeName(string name)
        => name.Trim().ToUpperInvariant();
}
