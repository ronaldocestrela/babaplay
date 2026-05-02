using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

/// <summary>
/// Represents a player profile registered within a tenant's sports association.
/// Lives in the per-tenant database; UserId references an ApplicationUser in the Master database.
/// </summary>
public sealed class Player : EntityBase
{
    /// <summary>Reference to the ApplicationUser in the Master database (no FK — cross-DB).</summary>
    public Guid UserId { get; private set; }

    /// <summary>Full name of the player.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Optional nickname or alias used in matches.</summary>
    public string? Nickname { get; private set; }

    /// <summary>Optional contact phone number.</summary>
    public string? Phone { get; private set; }

    /// <summary>Optional date of birth.</summary>
    public DateOnly? DateOfBirth { get; private set; }

    /// <summary>Whether the player is currently active (soft-delete flag).</summary>
    public bool IsActive { get; private set; }

    // Parameterless constructor required by EF Core.
    private Player() { }

    /// <summary>
    /// Creates a new active player. Throws <see cref="ValidationException"/> if
    /// <paramref name="userId"/> is empty or <paramref name="name"/> is null/whitespace.
    /// </summary>
    public static Player Create(
        Guid userId,
        string name,
        string? nickname,
        string? phone,
        DateOnly? dateOfBirth)
    {
        if (userId == Guid.Empty)
            throw new ValidationException("UserId", "UserId is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name", "Player name is required.");

        return new Player
        {
            UserId = userId,
            Name = name.Trim(),
            Nickname = nickname?.Trim(),
            Phone = phone?.Trim(),
            DateOfBirth = dateOfBirth,
            IsActive = true,
        };
    }

    /// <summary>
    /// Updates mutable profile fields. Throws <see cref="ValidationException"/>
    /// if <paramref name="name"/> is null or whitespace.
    /// </summary>
    public void Update(string name, string? nickname, string? phone, DateOnly? dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name", "Player name is required.");

        Name = name.Trim();
        Nickname = nickname?.Trim();
        Phone = phone?.Trim();
        DateOfBirth = dateOfBirth;
        MarkUpdated();
    }

    /// <summary>Soft-deletes the player by setting <see cref="IsActive"/> to false.</summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }
}
