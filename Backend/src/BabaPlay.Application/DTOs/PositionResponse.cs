namespace BabaPlay.Application.DTOs;

/// <summary>Data transfer object representing a tenant position.</summary>
public sealed record PositionResponse(
    Guid Id,
    Guid TenantId,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt);
