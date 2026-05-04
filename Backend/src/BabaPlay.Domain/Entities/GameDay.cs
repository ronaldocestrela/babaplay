using BabaPlay.Domain.Enums;
using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

/// <summary>
/// Represents a tenant-scoped game day schedule.
/// </summary>
public sealed class GameDay : EntityBase
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public DateTime ScheduledAt { get; private set; }
    public string? Location { get; private set; }
    public string? Description { get; private set; }
    public int MaxPlayers { get; private set; }
    public GameDayStatus Status { get; private set; }
    public bool IsActive { get; private set; }

    private GameDay() { }

    public static GameDay Create(
        Guid tenantId,
        string name,
        DateTime scheduledAt,
        string? location,
        string? description,
        int maxPlayers)
    {
        if (tenantId == Guid.Empty)
            throw new ValidationException("TenantId", "TenantId is required.");

        ValidateName(name);
        ValidateScheduledAt(scheduledAt);
        ValidateMaxPlayers(maxPlayers);

        var trimmedName = name.Trim();

        return new GameDay
        {
            TenantId = tenantId,
            Name = trimmedName,
            NormalizedName = NormalizeName(trimmedName),
            ScheduledAt = scheduledAt,
            Location = location?.Trim(),
            Description = description?.Trim(),
            MaxPlayers = maxPlayers,
            Status = GameDayStatus.Pending,
            IsActive = true,
        };
    }

    public void Update(string name, DateTime scheduledAt, string? location, string? description, int maxPlayers)
    {
        ValidateName(name);
        ValidateScheduledAt(scheduledAt);
        ValidateMaxPlayers(maxPlayers);

        Name = name.Trim();
        NormalizedName = NormalizeName(Name);
        ScheduledAt = scheduledAt;
        Location = location?.Trim();
        Description = description?.Trim();
        MaxPlayers = maxPlayers;
        MarkUpdated();
    }

    public void ChangeStatus(GameDayStatus newStatus)
    {
        if (Status == newStatus)
            return;

        var isValidTransition = Status switch
        {
            GameDayStatus.Pending => newStatus is GameDayStatus.Confirmed or GameDayStatus.Cancelled,
            GameDayStatus.Confirmed => newStatus is GameDayStatus.Completed or GameDayStatus.Cancelled,
            GameDayStatus.Cancelled => false,
            GameDayStatus.Completed => false,
            _ => false,
        };

        if (!isValidTransition)
            throw new ValidationException("Status", "Invalid game day status transition.");

        Status = newStatus;
        MarkUpdated();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        MarkUpdated();
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name", "Game day name is required.");
    }

    private static void ValidateScheduledAt(DateTime scheduledAt)
    {
        if (scheduledAt <= DateTime.UtcNow)
            throw new ValidationException("ScheduledAt", "ScheduledAt must be in the future.");
    }

    private static void ValidateMaxPlayers(int maxPlayers)
    {
        if (maxPlayers <= 0)
            throw new ValidationException("MaxPlayers", "MaxPlayers must be greater than zero.");
    }

    private static string NormalizeName(string name)
        => name.Trim().ToUpperInvariant();
}
