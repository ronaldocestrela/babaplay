namespace BabaPlay.Modules.Associates.Dtos;

/// <summary>Position link for API responses (no navigation back to Associate).</summary>
public sealed record AssociatePositionInfo(string PositionId, string PositionName);

/// <summary>Associate payload returned by the API (avoids EF navigation cycles in JSON).</summary>
public sealed record AssociateResponse(
    string Id,
    string Name,
    string? Email,
    string? Phone,
    string? UserId,
    bool IsActive,
    IReadOnlyList<AssociatePositionInfo> Positions,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
