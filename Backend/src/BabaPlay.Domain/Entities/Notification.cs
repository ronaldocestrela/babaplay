using BabaPlay.Domain.Enums;
using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

public sealed class Notification : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string? PayloadJson { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    private Notification() { }

    public static Notification Create(
        Guid tenantId,
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? payloadJson)
    {
        if (tenantId == Guid.Empty)
            throw new ValidationException("TenantId", "TenantId is required.");

        if (userId == Guid.Empty)
            throw new ValidationException("UserId", "UserId is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new ValidationException("Title", "Title is required.");

        if (string.IsNullOrWhiteSpace(message))
            throw new ValidationException("Message", "Message is required.");

        return new Notification
        {
            TenantId = tenantId,
            UserId = userId,
            Type = type,
            Title = title.Trim(),
            Message = message.Trim(),
            PayloadJson = string.IsNullOrWhiteSpace(payloadJson) ? null : payloadJson.Trim(),
            IsRead = false,
            ReadAtUtc = null,
            IsActive = true,
        };
    }

    public void MarkAsRead(DateTime readAtUtc)
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAtUtc = readAtUtc;
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
