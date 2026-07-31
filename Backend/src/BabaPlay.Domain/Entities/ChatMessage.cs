using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.Entities;

/// <summary>
/// Represents a message sent in the team/association chat (F27 — Real-Time Chat).
/// </summary>
public sealed class ChatMessage : EntityBase
{
    public Guid TenantId { get; private set; }
    public Guid SenderId { get; private set; }
    public string SenderName { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime SentAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    private ChatMessage() { }

    public static ChatMessage Create(
        Guid tenantId,
        Guid senderId,
        string senderName,
        string content)
    {
        if (tenantId == Guid.Empty)
            throw new ValidationException("TenantId", "TenantId is required.");

        if (senderId == Guid.Empty)
            throw new ValidationException("SenderId", "SenderId is required.");

        if (string.IsNullOrWhiteSpace(senderName))
            throw new ValidationException("SenderName", "SenderName is required.");

        if (string.IsNullOrWhiteSpace(content))
            throw new ValidationException("Content", "Content is required.");

        return new ChatMessage
        {
            TenantId = tenantId,
            SenderId = senderId,
            SenderName = senderName.Trim(),
            Content = content.Trim(),
            SentAtUtc = DateTime.UtcNow,
            IsActive = true,
        };
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        MarkUpdated();
    }
}
