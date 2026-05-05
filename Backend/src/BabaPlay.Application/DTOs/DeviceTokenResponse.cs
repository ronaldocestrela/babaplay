namespace BabaPlay.Application.DTOs;

public sealed record DeviceTokenResponse(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    string DeviceId,
    string Token,
    string Platform,
    bool IsActive);
