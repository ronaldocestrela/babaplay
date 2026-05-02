using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

/// <summary>
/// Many-to-many link between Role and Permission.
/// </summary>
public sealed class RolePermission
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private RolePermission() { }

    public static RolePermission Create(Guid roleId, Guid permissionId)
    {
        if (roleId == Guid.Empty)
            throw new ValidationException("RoleId", "RoleId is required.");

        if (permissionId == Guid.Empty)
            throw new ValidationException("PermissionId", "PermissionId is required.");

        return new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
        };
    }
}
