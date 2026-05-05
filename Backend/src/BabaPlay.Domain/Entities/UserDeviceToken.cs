using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

public sealed class UserDeviceToken : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string DeviceId { get; private set; } = string.Empty;
    public string Token { get; private set; } = string.Empty;
    public string Platform { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private UserDeviceToken() { }

    public static UserDeviceToken Create(Guid tenantId, Guid userId, string deviceId, string token, string platform)
    {
        if (tenantId == Guid.Empty)
            throw new ValidationException("TenantId", "TenantId is required.");

        if (userId == Guid.Empty)
            throw new ValidationException("UserId", "UserId is required.");

        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ValidationException("DeviceId", "DeviceId is required.");

        if (string.IsNullOrWhiteSpace(token))
            throw new ValidationException("Token", "Token is required.");

        if (string.IsNullOrWhiteSpace(platform))
            throw new ValidationException("Platform", "Platform is required.");

        return new UserDeviceToken
        {
            TenantId = tenantId,
            UserId = userId,
            DeviceId = deviceId.Trim(),
            Token = token.Trim(),
            Platform = platform.Trim().ToLowerInvariant(),
            IsActive = true,
        };
    }

    public void RotateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ValidationException("Token", "Token is required.");

        Token = token.Trim();
        MarkUpdated();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        MarkUpdated();
    }
}
