using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

/// <summary>
/// Tenant role assignment for a user id from the Master identity database.
/// </summary>
public sealed class UserRole
{
    public string UserId { get; private set; } = string.Empty;
    public Guid RoleId { get; private set; }
    public DateTime AssignedAt { get; private set; } = DateTime.UtcNow;

    private UserRole() { }

    public static UserRole Create(string userId, Guid roleId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ValidationException("UserId", "UserId is required.");

        if (roleId == Guid.Empty)
            throw new ValidationException("RoleId", "RoleId is required.");

        return new UserRole
        {
            UserId = userId.Trim(),
            RoleId = roleId,
        };
    }
}
