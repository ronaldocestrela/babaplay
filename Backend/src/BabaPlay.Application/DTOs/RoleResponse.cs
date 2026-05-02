namespace BabaPlay.Application.DTOs;

/// <summary>DTO representing a tenant-scoped role.</summary>
public sealed record RoleResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    IReadOnlyList<Guid> PermissionIds);
